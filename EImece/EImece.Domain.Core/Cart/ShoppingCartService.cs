using System.Text.Json;
using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Domain.Core.Payments;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EImece.Domain.Core.Cart;

public interface IShoppingCartService
{
    string GetOrCreateOrderGuid();
    Task<CartState> GetCartAsync(CancellationToken cancellationToken = default);
    Task<CartState> AddAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    Task<CartState> UpdateQuantityAsync(int productId, int quantity, CancellationToken cancellationToken = default);
    Task<CartState> RemoveLineAsync(int productId, CancellationToken cancellationToken = default);
    Task<(CartState Cart, string? Message)> ApplyCouponAsync(string couponCode, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
    Task PersistAsync(CartState cart, CancellationToken cancellationToken = default);
}

public sealed class CartLine
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public sealed class CartState
{
    public string OrderGuid { get; set; } = string.Empty;
    public List<CartLine> Lines { get; set; } = [];
    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Subtotal => Lines.Sum(l => l.LineTotal);
    public decimal Total => Math.Max(0, Subtotal - DiscountAmount);
}

public sealed class ShoppingCartService : IShoppingCartService
{
    public const string SessionKey = "EImece.Cart";
    public const string CookieOrderGuid = "EImece.OrderGuid";

    private readonly IHttpContextAccessor _http;
    private readonly EImeceDbContext _db;

    public ShoppingCartService(IHttpContextAccessor http, EImeceDbContext db)
    {
        _http = http;
        _db = db;
    }

    public string GetOrCreateOrderGuid()
    {
        var context = _http.HttpContext ?? throw new InvalidOperationException("No HttpContext");
        if (context.Request.Cookies.TryGetValue(CookieOrderGuid, out var existing) && !string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var guid = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(CookieOrderGuid, guid, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddDays(14),
            SameSite = SameSiteMode.Lax
        });
        return guid;
    }

    public Task<CartState> GetCartAsync(CancellationToken cancellationToken = default)
    {
        var session = _http.HttpContext?.Session;
        if (session is null)
        {
            return Task.FromResult(new CartState { OrderGuid = GetOrCreateOrderGuid() });
        }

        var json = session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return Task.FromResult(new CartState { OrderGuid = GetOrCreateOrderGuid() });
        }

        var cart = JsonSerializer.Deserialize<CartState>(json) ?? new CartState();
        if (string.IsNullOrWhiteSpace(cart.OrderGuid))
        {
            cart.OrderGuid = GetOrCreateOrderGuid();
        }

        return Task.FromResult(cart);
    }

    public async Task<CartState> AddAsync(int productId, int quantity, CancellationToken cancellationToken = default)
    {
        quantity = Math.Clamp(quantity, 1, 1000);
        var cart = await GetCartAsync(cancellationToken).ConfigureAwait(false);
        var product = await _db.Products.AsNoTracking()
            .Where(p => p.Id == productId && p.IsActive)
            .Select(p => new { p.Id, p.Name, p.ProductCode, p.Price })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (product is null)
        {
            return cart;
        }

        var line = cart.Lines.FirstOrDefault(l => l.ProductId == productId);
        if (line is null)
        {
            cart.Lines.Add(new CartLine
            {
                ProductId = product.Id,
                Name = product.Name,
                ProductCode = product.ProductCode,
                UnitPrice = product.Price,
                Quantity = quantity
            });
        }
        else
        {
            line.Quantity = Math.Clamp(line.Quantity + quantity, 1, 1000);
        }

        await PersistAsync(cart, cancellationToken).ConfigureAwait(false);
        return cart;
    }

    public async Task<CartState> UpdateQuantityAsync(int productId, int quantity, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(cancellationToken).ConfigureAwait(false);
        var line = cart.Lines.FirstOrDefault(l => l.ProductId == productId);
        if (line is null)
        {
            return cart;
        }

        if (quantity <= 0)
        {
            cart.Lines.Remove(line);
            if (cart.Lines.Count == 0)
            {
                cart.CouponCode = null;
                cart.DiscountAmount = 0;
            }
        }
        else
        {
            line.Quantity = Math.Clamp(quantity, 1, 1000);
        }

        await ReapplyCouponAsync(cart, cancellationToken).ConfigureAwait(false);
        await PersistAsync(cart, cancellationToken).ConfigureAwait(false);
        return cart;
    }

    public async Task<CartState> RemoveLineAsync(int productId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(cancellationToken).ConfigureAwait(false);
        cart.Lines.RemoveAll(l => l.ProductId == productId);
        if (cart.Lines.Count == 0)
        {
            cart.CouponCode = null;
            cart.DiscountAmount = 0;
        }
        else
        {
            await ReapplyCouponAsync(cart, cancellationToken).ConfigureAwait(false);
        }

        await PersistAsync(cart, cancellationToken).ConfigureAwait(false);
        return cart;
    }

    public async Task<(CartState Cart, string? Message)> ApplyCouponAsync(string couponCode, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(cancellationToken).ConfigureAwait(false);
        if (cart.Lines.Count == 0)
        {
            return (cart, "Sepet boş.");
        }

        if (string.IsNullOrWhiteSpace(couponCode))
        {
            cart.CouponCode = null;
            cart.DiscountAmount = 0;
            await PersistAsync(cart, cancellationToken).ConfigureAwait(false);
            return (cart, null);
        }

        var now = DateTime.UtcNow;
        var coupon = await _db.Coupons.AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.IsActive &&
                c.Code == couponCode.Trim() &&
                c.StartDate <= now &&
                c.EndDate >= now,
                cancellationToken)
            .ConfigureAwait(false);

        if (coupon is null)
        {
            cart.CouponCode = null;
            cart.DiscountAmount = 0;
            await PersistAsync(cart, cancellationToken).ConfigureAwait(false);
            return (cart, "Kupon geçersiz veya süresi dolmuş.");
        }

        cart.CouponCode = coupon.Code;
        cart.DiscountAmount = coupon.DiscountPercentage > 0
            ? Math.Round(cart.Subtotal * coupon.DiscountPercentage / 100m, 2)
            : coupon.Discount;

        await PersistAsync(cart, cancellationToken).ConfigureAwait(false);
        return (cart, $"Kupon uygulandı: {coupon.Code}");
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        var cart = new CartState { OrderGuid = GetOrCreateOrderGuid(), Lines = [] };
        await PersistAsync(cart, cancellationToken).ConfigureAwait(false);
    }

    private async Task ReapplyCouponAsync(CartState cart, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cart.CouponCode))
        {
            cart.DiscountAmount = 0;
            return;
        }

        var now = DateTime.UtcNow;
        var coupon = await _db.Coupons.AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.IsActive &&
                c.Code == cart.CouponCode &&
                c.StartDate <= now &&
                c.EndDate >= now,
                cancellationToken)
            .ConfigureAwait(false);

        if (coupon is null)
        {
            cart.CouponCode = null;
            cart.DiscountAmount = 0;
            return;
        }

        cart.DiscountAmount = coupon.DiscountPercentage > 0
            ? Math.Round(cart.Subtotal * coupon.DiscountPercentage / 100m, 2)
            : coupon.Discount;
    }

    public async Task PersistAsync(CartState cart, CancellationToken cancellationToken = default)
    {
        var session = _http.HttpContext?.Session;
        session?.SetString(SessionKey, JsonSerializer.Serialize(cart));

        var existing = await _db.ShoppingCarts.FirstOrDefaultAsync(c => c.OrderGuid == cart.OrderGuid, cancellationToken).ConfigureAwait(false);
        var json = JsonSerializer.Serialize(cart.Lines);
        if (existing is null)
        {
            _db.ShoppingCarts.Add(new ShoppingCart
            {
                Name = cart.OrderGuid,
                OrderGuid = cart.OrderGuid,
                ShoppingCartJson = json,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                Lang = 1
            });
        }
        else
        {
            existing.ShoppingCartJson = json;
            existing.UpdatedDate = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
