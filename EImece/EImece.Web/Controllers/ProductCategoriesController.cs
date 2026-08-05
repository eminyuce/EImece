using EImece.Domain.Core.Data;
using EImece.Domain.Core.Services;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class ProductCategoriesController : BaseController
{
    private readonly EImeceDbContext _db;

    public ProductCategoriesController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db)
        : base(siteOptions)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? id, CancellationToken cancellationToken)
    {
        if (!int.TryParse(id, out var categoryId))
        {
            categoryId = SeoIdParser.Parse(id);
        }

        if (categoryId <= 0)
        {
            var categories = await _db.ProductCategories.AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Position)
                .Take(50)
                .Select(c => new ProductListItemViewModel { Id = c.Id, Name = c.Name, CategoryId = c.Id })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return View(new CategoryShellViewModel
            {
                Name = "Kategoriler",
                Summary = $"{categories.Count} aktif kategori",
                Products = categories
            });
        }

        try
        {
            var category = await _db.ProductCategories.AsNoTracking()
                .Where(c => c.Id == categoryId)
                .Select(c => new { c.Id, c.Name, c.IsActive })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (category is null)
            {
                return View(new CategoryShellViewModel
                {
                    Id = categoryId,
                    Name = $"Category {categoryId}",
                    Summary = "Category not found."
                });
            }

            var products = await _db.Products.AsNoTracking()
                .Where(p => p.ProductCategoryId == categoryId && p.IsActive)
                .OrderBy(p => p.Position)
                .Take(48)
                .Select(p => new ProductListItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ProductCode = p.ProductCode,
                    CategoryId = categoryId,
                    CategoryName = category.Name,
                    CategorySlug = StorefrontMapping.Slug(category.Name)
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return View(new CategoryShellViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Summary = category.IsActive ? $"{products.Count} ürün" : "Inactive category",
                Products = products
            });
        }
        catch (Exception ex)
        {
            return View(new CategoryShellViewModel
            {
                Id = categoryId,
                Name = $"Category {categoryId}",
                Summary = "Database unavailable.",
                Notice = ex.Message
            });
        }
    }
}
