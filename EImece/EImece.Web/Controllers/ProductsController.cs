using EImece.Domain.Core.Data;
using EImece.Domain.Core.Services;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class ProductsController : BaseController
{
    private readonly IStorefrontService _storefront;
    private readonly EImeceDbContext _db;

    public ProductsController(IOptions<EImeceOptions> siteOptions, IStorefrontService storefront, EImeceDbContext db)
        : base(siteOptions)
    {
        _storefront = storefront;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Detail(string categoryName, string? id, CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(categoryName) ? "category" : categoryName;
        var productId = SeoIdParser.Parse(id);
        if (productId <= 0)
        {
            return View(new ProductDetailViewModel
            {
                Name = "Ürün",
                CategorySlug = slug,
                CategoryName = slug,
                Summary = "Geçersiz ürün kimliği."
            });
        }

        try
        {
            var product = await _storefront.GetProductDetailAsync(productId, cancellationToken).ConfigureAwait(false);
            if (product is null || !product.IsActive)
            {
                return View(new ProductDetailViewModel
                {
                    Id = productId,
                    Name = $"Ürün {productId}",
                    CategorySlug = slug,
                    CategoryName = slug,
                    Summary = "Ürün bulunamadı."
                });
            }

            var categoryNameResolved = product.ProductCategory?.Name ?? slug;
            return View(new ProductDetailViewModel
            {
                Id = product.Id,
                Name = product.Name,
                ProductCode = product.ProductCode,
                Price = product.Price,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                CategoryId = product.ProductCategoryId,
                CategorySlug = StorefrontMapping.Slug(categoryNameResolved),
                CategoryName = categoryNameResolved,
                Summary = product.ShortDescription
            });
        }
        catch (Exception ex)
        {
            return View(new ProductDetailViewModel
            {
                Id = productId,
                Name = $"Ürün {productId}",
                CategorySlug = slug,
                CategoryName = slug,
                Summary = "Veritabanı kullanılamıyor.",
                Notice = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Tag(string? id, CancellationToken cancellationToken)
    {
        var tagId = SeoIdParser.Parse(id);
        var model = new ProductTagViewModel { TagId = tagId, TagName = $"Etiket {tagId}" };
        if (tagId <= 0)
        {
            return View(model);
        }

        try
        {
            var tag = await _storefront.GetTagAsync(tagId, cancellationToken).ConfigureAwait(false);
            if (tag is not null)
            {
                model.TagName = tag.Name;
            }

            var products = await _storefront.GetProductsByTagAsync(tagId, SiteOptions.MainLanguage, cancellationToken).ConfigureAwait(false);
            model.Products = products.Select(StorefrontMapping.ToListItem).ToList();
        }
        catch
        {
            model.Products = Array.Empty<ProductListItemViewModel>();
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> SearchProducts(string? q, string? search, CancellationToken cancellationToken)
    {
        var query = string.IsNullOrWhiteSpace(q) ? search : q;
        var model = new ProductSearchViewModel { Query = query };
        try
        {
            var products = await _storefront.SearchProductsAsync(query, null, SiteOptions.MainLanguage, 48, cancellationToken).ConfigureAwait(false);
            model.Products = products.Select(StorefrontMapping.ToListItem).ToList();
        }
        catch
        {
            model.Products = Array.Empty<ProductListItemViewModel>();
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> AdvancedSearchProducts(string? search, string? categoryId, CancellationToken cancellationToken)
    {
        int? catId = int.TryParse(categoryId, out var parsed) ? parsed : null;
        var model = new ProductSearchViewModel { Query = search, CategoryId = catId };

        try
        {
            model.Categories = await _db.ProductCategories.AsNoTracking()
                .Where(c => c.IsActive && c.Lang == SiteOptions.MainLanguage)
                .OrderBy(c => c.Position)
                .Take(100)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var products = await _storefront.SearchProductsAsync(search, catId, SiteOptions.MainLanguage, 48, cancellationToken).ConfigureAwait(false);
            model.Products = products.Select(StorefrontMapping.ToListItem).ToList();
        }
        catch
        {
            model.Categories = Array.Empty<EImece.Domain.Core.Entities.ProductCategory>();
            model.Products = Array.Empty<ProductListItemViewModel>();
        }

        return View(model);
    }
}
