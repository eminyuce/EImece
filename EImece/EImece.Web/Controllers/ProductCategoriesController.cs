using EImece.Domain.Core.Data;
using EImece.Web.Configuration;
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
            return Placeholder("Category", "Category list shell — use /c/pc/{id}", new { id });
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
                return Placeholder("Category", $"Category {categoryId} not found (or database offline).", new { id });
            }

            return Placeholder(category.Name, $"Category #{category.Id} · Active={category.IsActive}", new { id });
        }
        catch
        {
            return Placeholder("Category", $"Route OK: /c/pc/{id} (database unavailable — shell only).", new { id });
        }
    }
}
