using EImece.Domain.Core.Data;
using EImece.Web.Configuration;
using EImece.Web.Helpers;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class ProductCommentsController : BaseAdminController
{
    private readonly EImeceDbContext _db;

    public ProductCommentsController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db) : base(siteOptions)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int id = 0, string? search = null, int page = 1, int pageSize = 25, string? sort = null, string? sortDir = null, CancellationToken cancellationToken = default)
    {
        if (id > 0)
        {
            var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
            if (product is null) return NotFound();

            var query = _db.ProductComments.AsNoTracking().Where(c => c.ProductId == id);
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    (c.Subject != null && c.Subject.Contains(search))
                    || (c.Review != null && c.Review.Contains(search))
                    || (c.Email != null && c.Email.Contains(search)));
            }

            var comments = await query.OrderByDescending(c => c.Id)
                .Select(c => new ProductCommentRow
                {
                    Id = c.Id,
                    Subject = c.Subject,
                    Review = c.Review,
                    Email = c.Email,
                    Rating = c.Rating,
                    IsActive = c.IsActive,
                    CreatedDate = c.CreatedDate
                })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            return View(new ProductCommentListViewModel
            {
                ProductId = id,
                ProductName = product.Name,
                Comments = comments
            });
        }

        var grid = GridQuery(search, page, pageSize, sort, sortDir);
        try
        {
            var allQuery = _db.ProductComments.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(grid.Search))
            {
                allQuery = allQuery.Where(c =>
                    (c.Subject != null && c.Subject.Contains(grid.Search))
                    || (c.Review != null && c.Review.Contains(grid.Search))
                    || (c.Email != null && c.Email.Contains(grid.Search)));
            }

            allQuery = (grid.Sort?.ToLowerInvariant()) switch
            {
                "productid" => grid.SortDir == "asc" ? allQuery.OrderBy(c => c.ProductId) : allQuery.OrderByDescending(c => c.ProductId),
                "subject" => grid.SortDir == "asc" ? allQuery.OrderBy(c => c.Subject) : allQuery.OrderByDescending(c => c.Subject),
                "email" => grid.SortDir == "asc" ? allQuery.OrderBy(c => c.Email) : allQuery.OrderByDescending(c => c.Email),
                "rating" => grid.SortDir == "asc" ? allQuery.OrderBy(c => c.Rating) : allQuery.OrderByDescending(c => c.Rating),
                "active" => grid.SortDir == "asc" ? allQuery.OrderBy(c => c.IsActive) : allQuery.OrderByDescending(c => c.IsActive),
                _ => allQuery.OrderByDescending(c => c.Id)
            };

            var total = await allQuery.CountAsync(cancellationToken).ConfigureAwait(false);
            var latest = await allQuery.ApplyPaging(grid)
                .Select(c => new { c.Id, c.ProductId, c.Subject, c.Email, c.Rating, c.IsActive })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var rows = latest.Select(x => (IReadOnlyList<string?>)new string?[]
            {
                x.Id.ToString(),
                x.ProductId.ToString(),
                x.Subject,
                x.Email,
                x.Rating.ToString(),
                x.IsActive ? "Evet" : "Hayır"
            });

            var model = BuildList(
                "Ürün Yorumları",
                "ProductComments",
                new[] { "Id", "Ürün Id", "Konu", "E-posta", "Puan", "Aktif" },
                rows,
                grid.Search,
                showCreate: false,
                totalCount: total,
                grid: grid,
                ajaxDeleteAction: "DeleteProductCommentGridItem");
            model.ShowEditButton = false;
            model.ShowDeleteButton = true;
            return EntityList(model);
        }
        catch (Exception ex)
        {
            var model = BuildList(
                "Ürün Yorumları",
                "ProductComments",
                new[] { "Id", "Ürün Id", "Konu", "E-posta", "Puan", "Aktif" },
                Array.Empty<IReadOnlyList<string?>>(),
                grid.Search,
                ex.Message,
                showCreate: false,
                grid: grid);
            model.ShowEditButton = false;
            model.ShowDeleteButton = true;
            return EntityList(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var comment = await _db.ProductComments.FirstOrDefaultAsync(c => c.Id == id, cancellationToken).ConfigureAwait(false);
        if (comment is null) return NotFound();

        var productId = comment.ProductId;
        _db.ProductComments.Remove(comment);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Yorum silindi");
        return RedirectToAction(nameof(Index), new { id = productId });
    }
}
