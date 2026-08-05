using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EImece.Domain.Core.Admin;

public interface IProductAdminService
{
    Task<List<CategoryTreeNode>> BuildCategoryTreeAsync(int lang, CancellationToken ct = default);
    Task<(List<ProductAdminRow> Items, int Total)> GetProductsAsync(
        int categoryId, int brandId, string? search, int page, int pageSize, string? sort, string sortDir, int lang, CancellationToken ct = default);
    Task ApplyOrderingOrStateAsync(List<OrderingItem> values, string? checkbox, CancellationToken ct = default);
    Task ChangeProductStateAsync(IEnumerable<string> ids, ProductState state, CancellationToken ct = default);
    Task SoftDeleteAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task MoveProductsAsync(int newCategoryId, IEnumerable<int> productIds, CancellationToken ct = default);
}

public sealed class ProductAdminRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameLong { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? Discount { get; set; }
    public string State { get; set; } = string.Empty;
    public int Position { get; set; }
    public bool IsActive { get; set; }
    public bool MainPage { get; set; }
    public bool IsCampaign { get; set; }
    public bool ImageState { get; set; }
    public int? MainImageId { get; set; }
    public int ProductCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public int? BrandId { get; set; }
    public int CommentCount { get; set; }
    public int? TemplateId { get; set; }

    public bool HasDiscount => Discount.HasValue && Discount.Value > 0 && Discount.Value < Price;
    public decimal PriceWithDiscount => HasDiscount ? Discount!.Value : Price;
    public int DiscountPercentage => !HasDiscount || Price <= 0
        ? 0
        : (int)Math.Round((Price - PriceWithDiscount) / Price * 100m);
    public ProductState StateEnum => ProductStateLabels.Parse(State);
    public string StateLabel => ProductStateLabels.ToTurkish(StateEnum);
    public string SeoSlug => $"{Slugify(CategoryName)}-{Id}";
    public string CategorySlug => Slugify(CategoryName);

    private static string Slugify(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "urun";
        var t = text.ToLowerInvariant()
            .Replace('ı', 'i').Replace('ğ', 'g').Replace('ü', 'u')
            .Replace('ş', 's').Replace('ö', 'o').Replace('ç', 'c');
        var chars = t.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        return new string(chars).Trim('-');
    }
}

public sealed class ProductAdminService : IProductAdminService
{
    private readonly EImeceDbContext _db;

    public ProductAdminService(EImeceDbContext db) => _db = db;

    public async Task<List<CategoryTreeNode>> BuildCategoryTreeAsync(int lang, CancellationToken ct = default)
    {
        var categories = await _db.ProductCategories.AsNoTracking()
            .Where(c => c.Lang == lang || lang == 0)
            .Select(c => new { c.Id, c.Name, c.ParentId, c.Position })
            .OrderBy(c => c.Position).ThenBy(c => c.Name)
            .ToListAsync(ct).ConfigureAwait(false);

        var counts = await _db.Products.AsNoTracking()
            .GroupBy(p => p.ProductCategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToListAsync(ct).ConfigureAwait(false);
        var countMap = counts.ToDictionary(x => x.CategoryId, x => x.Count);

        var nodes = categories.Select(c => new CategoryTreeNode
        {
            Id = c.Id,
            Name = c.Name,
            ParentId = c.ParentId,
            ProductCount = countMap.GetValueOrDefault(c.Id)
        }).ToList();

        var lookup = nodes.ToLookup(n => n.ParentId);
        void Attach(CategoryTreeNode node, int level)
        {
            node.Level = level;
            node.Children = lookup[node.Id].OrderBy(c => c.Name).ToList();
            foreach (var child in node.Children) Attach(child, level + 1);
        }

        var roots = lookup[0].Concat(lookup.Where(g => g.Key != 0 && nodes.All(n => n.Id != g.Key)).SelectMany(g => g))
            .DistinctBy(n => n.Id)
            .OrderBy(n => n.Name)
            .ToList();
        // Prefer ParentId == 0 as roots
        roots = lookup[0].OrderBy(n => n.Name).ToList();
        if (roots.Count == 0)
            roots = nodes.Where(n => nodes.All(p => p.Id != n.ParentId)).OrderBy(n => n.Name).ToList();

        foreach (var root in roots) Attach(root, 1);
        return roots;
    }

    public async Task<(List<ProductAdminRow> Items, int Total)> GetProductsAsync(
        int categoryId, int brandId, string? search, int page, int pageSize, string? sort, string sortDir, int lang, CancellationToken ct = default)
    {
        var query = _db.Products.AsNoTracking()
            .Include(p => p.ProductCategory)
            .Include(p => p.Brand)
            .AsQueryable();

        if (lang > 0) query = query.Where(p => p.Lang == lang);
        if (categoryId > 0)
        {
            var categoryIds = await GetCategoryAndDescendantIdsAsync(categoryId, ct).ConfigureAwait(false);
            query = query.Where(p => categoryIds.Contains(p.ProductCategoryId));
        }
        if (brandId > 0) query = query.Where(p => p.BrandId == brandId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => p.Name.Contains(s) || p.ProductCode.Contains(s) || (p.NameLong != null && p.NameLong.Contains(s)));
        }

        query = (sort?.ToLowerInvariant()) switch
        {
            "name" or "isim" => sortDir == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            "price" or "fiyat" => sortDir == "asc" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
            "position" or "sırası" => sortDir == "asc" ? query.OrderBy(p => p.Position) : query.OrderByDescending(p => p.Position),
            "code" => sortDir == "asc" ? query.OrderBy(p => p.ProductCode) : query.OrderByDescending(p => p.ProductCode),
            _ => query.OrderByDescending(p => p.Id)
        };

        var total = await query.CountAsync(ct).ConfigureAwait(false);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new ProductAdminRow
            {
                Id = p.Id,
                Name = p.Name,
                NameLong = p.NameLong,
                ProductCode = p.ProductCode,
                Price = p.Price,
                Discount = p.Discount,
                State = p.State,
                Position = p.Position,
                IsActive = p.IsActive,
                MainPage = p.MainPage,
                IsCampaign = p.IsCampaign,
                ImageState = p.ImageState,
                MainImageId = p.MainImageId,
                ProductCategoryId = p.ProductCategoryId,
                CategoryName = p.ProductCategory != null ? p.ProductCategory.Name : "",
                BrandName = p.Brand != null ? p.Brand.Name : null,
                BrandId = p.BrandId,
                CommentCount = p.ProductComments.Count,
                TemplateId = p.ProductCategory != null ? p.ProductCategory.TemplateId : null
            })
            .ToListAsync(ct).ConfigureAwait(false);

        return (items, total);
    }

    public async Task ApplyOrderingOrStateAsync(List<OrderingItem> values, string? checkbox, CancellationToken ct = default)
    {
        if (values.Count == 0) return;
        var ids = values.Select(v => v.Id).ToList();
        var products = await _db.Products.Where(p => ids.Contains(p.Id)).ToListAsync(ct).ConfigureAwait(false);
        foreach (var item in values)
        {
            var product = products.FirstOrDefault(p => p.Id == item.Id);
            if (product is null) continue;
            if (string.IsNullOrEmpty(checkbox))
            {
                product.Position = item.Position;
            }
            else if (checkbox.Equals("State", StringComparison.OrdinalIgnoreCase))
            {
                product.IsActive = item.IsActive;
            }
            else if (checkbox.Equals("MainPage", StringComparison.OrdinalIgnoreCase))
            {
                product.MainPage = item.IsActive;
            }
            else if (checkbox.Equals("ImageState", StringComparison.OrdinalIgnoreCase))
            {
                product.ImageState = item.IsActive;
            }
            else if (checkbox.Equals("IsCampaign", StringComparison.OrdinalIgnoreCase))
            {
                product.IsCampaign = item.IsActive;
            }
            product.UpdatedDate = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task ChangeProductStateAsync(IEnumerable<string> ids, ProductState state, CancellationToken ct = default)
    {
        var idList = ids.Select(s => int.TryParse(s, out var id) ? id : 0).Where(id => id > 0).ToList();
        var products = await _db.Products.Where(p => idList.Contains(p.Id)).ToListAsync(ct).ConfigureAwait(false);
        foreach (var p in products)
        {
            p.State = state.ToString();
            p.UpdatedDate = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task SoftDeleteAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        var products = await _db.Products.Where(p => idList.Contains(p.Id)).ToListAsync(ct).ConfigureAwait(false);
        foreach (var p in products)
        {
            p.IsActive = false;
            p.UpdatedDate = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task MoveProductsAsync(int newCategoryId, IEnumerable<int> productIds, CancellationToken ct = default)
    {
        var idList = productIds.ToList();
        var products = await _db.Products.Where(p => idList.Contains(p.Id)).ToListAsync(ct).ConfigureAwait(false);
        foreach (var p in products)
        {
            p.ProductCategoryId = newCategoryId;
            p.UpdatedDate = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task<HashSet<int>> GetCategoryAndDescendantIdsAsync(int categoryId, CancellationToken ct)
    {
        var all = await _db.ProductCategories.AsNoTracking()
            .Select(c => new { c.Id, c.ParentId }).ToListAsync(ct).ConfigureAwait(false);
        var result = new HashSet<int> { categoryId };
        bool added;
        do
        {
            added = false;
            foreach (var c in all)
            {
                if (result.Contains(c.ParentId) && result.Add(c.Id)) added = true;
            }
        } while (added);
        return result;
    }
}
