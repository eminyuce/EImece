using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EImece.Domain.Core.Services;

public sealed class StorefrontService : IStorefrontService
{
    private readonly EImeceDbContext _db;

    public StorefrontService(EImeceDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<MainPageImage>> GetMainPageBannersAsync(int lang, CancellationToken cancellationToken = default)
        => await _db.MainPageImages.AsNoTracking()
            .Where(m => m.IsActive && m.Lang == lang)
            .OrderBy(m => m.Position)
            .Take(12)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Product>> GetHomeProductsAsync(int lang, int take, CancellationToken cancellationToken = default)
    {
        var mainPage = await _db.Products.AsNoTracking()
            .Include(p => p.ProductCategory)
            .Where(p => p.IsActive && p.Lang == lang && p.MainPage)
            .OrderBy(p => p.Position)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (mainPage.Count > 0)
        {
            return mainPage;
        }

        return await _db.Products.AsNoTracking()
            .Include(p => p.ProductCategory)
            .Where(p => p.IsActive && p.Lang == lang)
            .OrderByDescending(p => p.Id)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<Product?> GetProductDetailAsync(int productId, CancellationToken cancellationToken = default)
        => _db.Products.AsNoTracking()
            .Include(p => p.ProductCategory)
            .Include(p => p.ProductFiles)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetProductsByTagAsync(int tagId, int lang, CancellationToken cancellationToken = default)
        => await _db.Products.AsNoTracking()
            .Include(p => p.ProductCategory)
            .Where(p => p.IsActive && p.Lang == lang && p.ProductTags.Any(t => t.TagId == tagId))
            .OrderBy(p => p.Position)
            .Take(48)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Product>> SearchProductsAsync(string? query, int? categoryId, int lang, int take, CancellationToken cancellationToken = default)
    {
        var q = _db.Products.AsNoTracking()
            .Include(p => p.ProductCategory)
            .Where(p => p.IsActive && p.Lang == lang);

        if (categoryId is > 0)
        {
            q = q.Where(p => p.ProductCategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            q = q.Where(p => p.Name.Contains(query) || p.ProductCode.Contains(query));
        }

        return await q.OrderByDescending(p => p.Id)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<Tag?> GetTagAsync(int tagId, CancellationToken cancellationToken = default)
        => _db.Tags.AsNoTracking()
            .Include(t => t.TagCategory)
            .FirstOrDefaultAsync(t => t.Id == tagId && t.IsActive, cancellationToken);

    public Task<Story?> GetStoryDetailAsync(int storyId, CancellationToken cancellationToken = default)
        => _db.Stories.AsNoTracking()
            .Include(s => s.StoryCategory)
            .Include(s => s.StoryFiles)
            .FirstOrDefaultAsync(s => s.Id == storyId, cancellationToken);

    public Task<StoryCategory?> GetStoryCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        => _db.StoryCategories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

    public async Task<IReadOnlyList<Story>> GetStoriesByCategoryAsync(int categoryId, int lang, CancellationToken cancellationToken = default)
        => await _db.Stories.AsNoTracking()
            .Include(s => s.StoryCategory)
            .Where(s => s.IsActive && s.Lang == lang && s.StoryCategoryId == categoryId)
            .OrderBy(s => s.Position)
            .Take(48)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Story>> GetStoriesByTagAsync(int tagId, int lang, CancellationToken cancellationToken = default)
        => await _db.Stories.AsNoTracking()
            .Include(s => s.StoryCategory)
            .Where(s => s.IsActive && s.Lang == lang && s.StoryTags.Any(t => t.TagId == tagId))
            .OrderByDescending(s => s.CreatedDate)
            .Take(48)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<Menu?> GetMenuPageAsync(int menuId, CancellationToken cancellationToken = default)
        => _db.Menus.AsNoTracking()
            .Include(m => m.MainImage)
            .FirstOrDefaultAsync(m => m.Id == menuId, cancellationToken);

    public Task<Menu?> GetMenuPageByLinkAsync(string menuLink, int lang, CancellationToken cancellationToken = default)
        => _db.Menus.AsNoTracking()
            .Include(m => m.MainImage)
            .FirstOrDefaultAsync(m => m.IsActive && m.Lang == lang &&
                (m.MenuLink == menuLink || m.Link == menuLink), cancellationToken);

    public async Task<IReadOnlyList<(string Loc, DateTime? LastMod)>> GetSitemapUrlsAsync(CancellationToken cancellationToken = default)
    {
        var urls = new List<(string Loc, DateTime? LastMod)>
        {
            ("/", null),
            ($"/{RoutePrefixes.Products}/arama/", null)
        };

        var products = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => new { p.Id, p.UpdatedDate, Category = p.ProductCategory != null ? p.ProductCategory.Name : "urun" })
            .Take(5000)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var p in products)
        {
            urls.Add(($"/{RoutePrefixes.Products}/{Slug(p.Category)}/{p.Id}/", p.UpdatedDate));
        }

        var categories = await _db.ProductCategories.AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new { c.Id, c.UpdatedDate })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var c in categories)
        {
            urls.Add(($"/{RoutePrefixes.Categories}/pc/{c.Id}/", c.UpdatedDate));
        }

        var stories = await _db.Stories.AsNoTracking()
            .Where(s => s.IsActive)
            .Select(s => new { s.Id, s.UpdatedDate, Category = s.StoryCategory != null ? s.StoryCategory.Name : "hikaye" })
            .Take(2000)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var s in stories)
        {
            urls.Add(($"/{RoutePrefixes.Stories}/{Slug(s.Category)}/{s.Id}/", s.UpdatedDate));
        }

        var pages = await _db.Menus.AsNoTracking()
            .Where(m => m.IsActive && m.LinkIsActive)
            .Select(m => new { m.Id, m.UpdatedDate })
            .Take(500)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var page in pages)
        {
            urls.Add(($"/{RoutePrefixes.Pages}/{page.Id}/", page.UpdatedDate));
        }

        return urls;
    }

    public async Task<IReadOnlyList<Product>> GetProductsForRssAsync(int take, int lang, int? categoryId, CancellationToken cancellationToken = default)
    {
        var q = _db.Products.AsNoTracking()
            .Include(p => p.ProductCategory)
            .Where(p => p.IsActive && p.Lang == lang);

        if (categoryId is > 0)
        {
            q = q.Where(p => p.ProductCategoryId == categoryId);
        }

        return await q.OrderByDescending(p => p.UpdatedDate)
            .Take(Math.Clamp(take, 1, 200))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Story>> GetStoriesForRssAsync(int take, int lang, int? categoryId, CancellationToken cancellationToken = default)
    {
        var q = _db.Stories.AsNoTracking()
            .Include(s => s.StoryCategory)
            .Where(s => s.IsActive && s.Lang == lang);

        if (categoryId is > 0)
        {
            q = q.Where(s => s.StoryCategoryId == categoryId);
        }

        return await q.OrderByDescending(s => s.UpdatedDate)
            .Take(Math.Clamp(take, 1, 200))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<Order?> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
        => _db.Orders.AsNoTracking()
            .Include(o => o.OrderProducts)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string userId, CancellationToken cancellationToken = default)
        => await _db.Orders.AsNoTracking()
            .Include(o => o.OrderProducts)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedDate)
            .Take(50)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    private static string Slug(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "item";
        }

        var chars = name.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        var slug = new string(chars).Trim('-');
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return string.IsNullOrEmpty(slug) ? "item" : slug;
    }

    private static class RoutePrefixes
    {
        public const string Products = "p";
        public const string Categories = "c";
        public const string Stories = "s";
        public const string Pages = "i";
    }
}
