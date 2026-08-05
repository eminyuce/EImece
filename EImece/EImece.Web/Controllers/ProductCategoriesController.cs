using EImece.Domain.Core.Data;
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
            return View(new CategoryShellViewModel
            {
                Name = "Categories",
                Summary = "Category list shell — use /c/pc/{id}.",
                Notice = "Provide a numeric category id."
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
                    Summary = "Category not found (or database offline).",
                    Notice = $"Route OK: /c/pc/{categoryId}"
                });
            }

            return View(new CategoryShellViewModel
            {
                Id = category.Id,
                Name = category.Name ?? $"Category {category.Id}",
                Summary = category.IsActive ? null : "This category is inactive."
            });
        }
        catch
        {
            return View(new CategoryShellViewModel
            {
                Id = categoryId,
                Name = $"Category {categoryId}",
                Summary = "Database unavailable — presentation shell only.",
                Notice = $"Route OK: /c/pc/{id}"
            });
        }
    }
}
