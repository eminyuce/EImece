using EImece.Domain.Core.Cart;
using EImece.Domain.Core.Data;
using EImece.Domain.Core.Email;
using EImece.Domain.Core.Entities;
using EImece.Domain.Core.Payments;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class PaymentController : BaseController
{
    private readonly IIyzicoPaymentService _iyzico;
    private readonly IEmailSender _email;
    private readonly IEmailTemplateRenderer _templates;
    private readonly IShoppingCartService _cart;
    private readonly EImeceDbContext _db;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IOptions<EImeceOptions> siteOptions,
        IIyzicoPaymentService iyzico,
        IEmailSender email,
        IEmailTemplateRenderer templates,
        IShoppingCartService cart,
        EImeceDbContext db,
        ILogger<PaymentController> logger)
        : base(siteOptions)
    {
        _iyzico = iyzico;
        _email = email;
        _templates = templates;
        _cart = cart;
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var cart = await _cart.GetCartAsync(cancellationToken).ConfigureAwait(false);
        return View(cart);
    }

    [HttpGet]
    public Task<IActionResult> ShoppingCart(CancellationToken cancellationToken) => Index(cancellationToken);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1, CancellationToken cancellationToken = default)
    {
        await _cart.AddAsync(productId, quantity, cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        await _cart.ClearAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int productId, int quantity, CancellationToken cancellationToken)
    {
        await _cart.UpdateQuantityAsync(productId, quantity, cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveCart(int productId, CancellationToken cancellationToken)
    {
        await _cart.RemoveLineAsync(productId, cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyCoupon(string couponCode, CancellationToken cancellationToken)
    {
        var (_, message) = await _cart.ApplyCouponAsync(couponCode, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(message))
        {
            TempData["CartNotice"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult CargoTracking(string? id)
    {
        return View(new CargoTrackingViewModel { OrderNumber = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CargoTrackingResult(string orderNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(orderNumber) || orderNumber.Length > 50)
        {
            return PartialView("_CargoTrackingResult", null);
        }

        var order = await _db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber.Trim(), cancellationToken)
            .ConfigureAwait(false);

        return PartialView("_CargoTrackingResult", order);
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> CheckoutBillingDetails(CancellationToken cancellationToken)
    {
        var cart = await _cart.GetCartAsync(cancellationToken).ConfigureAwait(false);
        if (cart.Lines.Count == 0)
        {
            return RedirectToAction(nameof(Index));
        }

        var customer = await _db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == User.Identity!.Name, cancellationToken)
            .ConfigureAwait(false);

        return View(new CheckoutBillingViewModel
        {
            Cart = cart,
            Customer = customer ?? new Customer { Country = "Turkey" }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> CheckoutBillingDetails(Customer customer, CancellationToken cancellationToken)
    {
        var cart = await _cart.GetCartAsync(cancellationToken).ConfigureAwait(false);
        if (cart.Lines.Count == 0)
        {
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(customer.Name) || string.IsNullOrWhiteSpace(customer.Email))
        {
            return View(new CheckoutBillingViewModel
            {
                Cart = cart,
                Customer = customer,
                Notice = "Ad ve e-posta zorunludur."
            });
        }

        customer.UpdatedDate = DateTime.UtcNow;
        if (customer.Id == 0)
        {
            customer.CreatedDate = DateTime.UtcNow;
            customer.IsActive = true;
            customer.Lang = SiteOptions.MainLanguage;
            _db.Customers.Add(customer);
        }
        else
        {
            _db.Customers.Update(customer);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return RedirectToAction(nameof(Checkout));
    }

    [HttpGet]
    public async Task<IActionResult> ThankYou(int orderId, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.OrderProducts)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            .ConfigureAwait(false);

        if (order is null)
        {
            return NotFound();
        }

        return View(new ThankYouViewModel { Order = order });
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        var model = await BuildCheckoutViewModelAsync(cancellationToken).ConfigureAwait(false);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> BuyNow(string categoryName, string? id, CancellationToken cancellationToken)
    {
        if (int.TryParse(id, out var productId))
        {
            await _cart.ClearAsync(cancellationToken).ConfigureAwait(false);
            await _cart.AddAsync(productId, 1, cancellationToken).ConfigureAwait(false);
        }

        var model = await BuildCheckoutViewModelAsync(cancellationToken).ConfigureAwait(false);
        model.Title = "Buy now";
        model.CategoryName = categoryName;
        model.ProductId = id;
        return View("Checkout", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CancellationToken cancellationToken)
    {
        var model = await BuildCheckoutViewModelAsync(cancellationToken).ConfigureAwait(false);
        if (model.BasketItems.Count == 0)
        {
            model.Notice = "Sepet boş.";
            return View("Checkout", model);
        }

        var cart = await _cart.GetCartAsync(cancellationToken).ConfigureAwait(false);
        var order = new Order
        {
            Name = "Web Order",
            OrderGuid = cart.OrderGuid,
            OrderNumber = "ORD-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            PaymentStatus = "PENDING",
            Price = cart.Total.ToString("0.00"),
            PaidPrice = cart.Total.ToString("0.00"),
            Currency = "TRY",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            Lang = SiteOptions.MainLanguage,
            DeliveryDate = DateTime.UtcNow.AddDays(3)
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var line in cart.Lines)
        {
            _db.OrderProducts.Add(new OrderProduct
            {
                OrderId = order.Id,
                ProductId = line.ProductId,
                ProductName = line.Name,
                ProductCode = line.ProductCode,
                Quantity = line.Quantity,
                Price = line.UnitPrice,
                ProductSalePrice = line.UnitPrice,
                TotalPrice = line.LineTotal
            });
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (!_iyzico.IsConfigured)
        {
            order.PaymentStatus = "DEMO_SUCCESS";
            order.UpdatedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await _cart.ClearAsync(cancellationToken).ConfigureAwait(false);
            return RedirectToAction(nameof(ThankYou), new { orderId = order.Id });
        }

        var callbackUrl = Url.Action(nameof(PaymentResult), "Payment", new { o = order.OrderGuid }, Request.Scheme, Request.Host.Value) ?? string.Empty;
        var request = new CheckoutInitializeRequest
        {
            OrderGuid = order.OrderGuid!,
            ConversationId = order.OrderNumber!,
            CallbackUrl = callbackUrl,
            Buyer = model.Buyer,
            ShippingAddress = model.Address,
            BillingAddress = model.Address,
            BasketItems = model.BasketItems,
            Price = cart.Total,
            PaidPrice = cart.Total
        };

        var result = await _iyzico.InitializeCheckoutFormAsync(request, cancellationToken).ConfigureAwait(false);
        model.InitializeResult = result;
        model.OrderGuid = order.OrderGuid;
        if (!result.Success)
        {
            model.Notice = result.ErrorMessage ?? "Iyzico initialize failed.";
            _logger.LogWarning("Iyzico PlaceOrder failed: {Error}", result.ErrorMessage);
        }

        return View("Checkout", model);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> PaymentResult(string? token, string? o, CancellationToken cancellationToken)
    {
        var vm = new PaymentResultViewModel { OrderGuid = o, Token = token };
        if (string.IsNullOrWhiteSpace(token))
        {
            vm.Success = false;
            vm.Message = "Missing Iyzico token.";
            return View(vm);
        }

        if (!_iyzico.IsConfigured)
        {
            vm.Success = false;
            vm.Message = "Iyzico is not configured on this host.";
            return View(vm);
        }

        var result = await _iyzico.RetrieveCheckoutFormAsync(token, cancellationToken).ConfigureAwait(false);
        vm.RetrieveResult = result;
        vm.Success = result.Success && string.Equals(result.PaymentStatus, "SUCCESS", StringComparison.OrdinalIgnoreCase);
        vm.Message = vm.Success ? "Payment successful." : (result.ErrorMessage ?? result.PaymentStatus ?? "Payment not successful.");

        if (!string.IsNullOrWhiteSpace(o))
        {
            var order = _db.Orders.FirstOrDefault(x => x.OrderGuid == o);
            if (order is not null)
            {
                order.PaymentStatus = result.PaymentStatus ?? (vm.Success ? "SUCCESS" : "FAILED");
                order.PaymentId = result.PaymentId;
                order.UpdatedDate = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        if (vm.Success)
        {
            await _cart.ClearAsync(cancellationToken).ConfigureAwait(false);
            var body = await _templates.RenderAsync(
                "<p>Order <strong>{{ OrderGuid }}</strong> payment {{ PaymentId }} confirmed.</p>",
                new { OrderGuid = o, PaymentId = result.PaymentId },
                cancellationToken).ConfigureAwait(false);
            await _email.SendAsync(new EmailMessage
            {
                ToAddress = "demo@eimece.local",
                Subject = $"Order confirmation {result.ConversationId}",
                HtmlBody = body
            }, cancellationToken).ConfigureAwait(false);
        }

        return View(vm);
    }

    private async Task<PaymentCheckoutViewModel> BuildCheckoutViewModelAsync(CancellationToken cancellationToken)
    {
        var cart = await _cart.GetCartAsync(cancellationToken).ConfigureAwait(false);
        var items = cart.Lines.Select(l => new CheckoutBasketItem
        {
            Id = l.ProductId.ToString(),
            Name = l.Name,
            Category1 = "General",
            Price = l.LineTotal
        }).ToList();

        if (items.Count == 0)
        {
            items.Add(new CheckoutBasketItem
            {
                Id = "DEMO-1",
                Name = "EImece demo product",
                Category1 = "Demo",
                Price = 10m
            });
        }

        return new PaymentCheckoutViewModel
        {
            Title = "Checkout",
            IsIyzicoConfigured = _iyzico.IsConfigured,
            IyzicoBaseUrl = _iyzico.BaseUrl,
            BasketItems = items,
            Buyer = new CheckoutBuyer(),
            Address = new CheckoutAddress(),
            OrderGuid = cart.OrderGuid,
            Notice = cart.Lines.Count == 0
                ? "Sepet boş — demo ürün ile checkout denenebilir."
                : $"{cart.Lines.Count} kalem · Toplam {cart.Total:N2} ₺"
        };
    }
}
