using EImece.Domain.Core.Data;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

/// <summary>
/// Storefront products — SEO routes under /p/... Full service logic migrates with Domain.Core services.
/// </summary>
public sealed class ProductsController : BaseController
{
    private readonly EImeceDbContext _db;

    public ProductsController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db)
        : base(siteOptions)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Detail(string categoryName, string? id, CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(categoryName) ? "category" : categoryName;

        if (!int.TryParse(id, out var productId))
        {
            return View(new ProductDetailViewModel
            {
                Name = "Product",
                CategorySlug = slug,
                CategoryName = slug,
                Summary = $"Invalid product id for category '{slug}'.",
                Notice = "Shell view — route parsing failed."
            });
        }

        try
        {
            var product = await _db.Products.AsNoTracking()
                .Where(p => p.Id == productId)
                .Select(p => new { p.Id, p.Name, p.ProductCode, p.Price, p.IsActive })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (product is null)
            {
                return View(new ProductDetailViewModel
                {
                    Id = productId,
                    Name = $"Product {productId}",
                    CategorySlug = slug,
                    CategoryName = slug,
                    Summary = "Product not found (or database offline).",
                    Notice = $"Route OK: /p/{slug}/{productId}"
                });
            }

            return View(new ProductDetailViewModel
            {
                Id = product.Id,
                Name = product.Name ?? $"Product {product.Id}",
                ProductCode = product.ProductCode,
                Price = product.Price,
                CategorySlug = slug,
                CategoryName = slug,
                Summary = product.IsActive ? null : "This product is inactive.",
                Notice = null
            });
        }
        catch
        {
            return View(new ProductDetailViewModel
            {
                Id = productId,
                Name = $"Product {productId}",
                CategorySlug = slug,
                CategoryName = slug,
                Summary = "Database unavailable — presentation shell only.",
                Notice = $"Route OK: /p/{slug}/{id}"
            });
        }
    }

    [HttpGet]
    public IActionResult Tag(string? id)
        => Placeholder("Product tag", $"Tag route /p/t/{id}", new { id });

    [HttpGet]
    public IActionResult SearchProducts(string? q)
    {
        ViewData["Query"] = q;
        return View();
    }

    [HttpGet]
    public IActionResult AdvancedSearchProducts()
        => Placeholder("Advanced search", "Advanced search route /p/advancedsearchproducts");
}
