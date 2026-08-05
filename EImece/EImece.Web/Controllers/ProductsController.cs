using EImece.Domain.Core.Data;
using EImece.Web.Configuration;
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
        if (!int.TryParse(id, out var productId))
        {
            return Placeholder("Product", $"Invalid product id for category '{categoryName}'.", new { categoryName, id });
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
                return Placeholder("Product", $"Product {productId} not found (or database offline).", new { categoryName, id });
            }

            return Placeholder(
                product.Name,
                $"Product #{product.Id} · {product.ProductCode} · {product.Price:0.00} · Active={product.IsActive}",
                new { categoryName, id, product.Id });
        }
        catch
        {
            return Placeholder("Product", $"Route OK: /p/{categoryName}/{id} (database unavailable — shell only).", new { categoryName, id });
        }
    }

    [HttpGet]
    public IActionResult Tag(string? id)
        => Placeholder("Product tag", $"Tag route /p/t/{id}", new { id });

    [HttpGet]
    public IActionResult SearchProducts(string? q)
        => Placeholder("Search products", $"Search route /p/arama · q={q}", new { q });

    [HttpGet]
    public IActionResult AdvancedSearchProducts()
        => Placeholder("Advanced search", "Advanced search route /p/advancedsearchproducts");
}
