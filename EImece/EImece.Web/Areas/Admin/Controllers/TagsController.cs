using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Web.Configuration;
using EImece.Web.Helpers;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class TagsController : BaseAdminController
{
    private readonly EImeceDbContext _db;

    public TagsController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db) : base(siteOptions)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 25, string? sort = null, string? sortDir = null, CancellationToken cancellationToken = default)
    {
        var grid = GridQuery(search, page, pageSize, sort, sortDir);
        try
        {
            var query = _db.Tags.AsNoTracking()
                .Where(p => string.IsNullOrWhiteSpace(grid.Search) || (p.Name != null && p.Name.Contains(grid.Search)));

            query = (grid.Sort?.ToLowerInvariant()) switch
            {
                "name" => grid.SortDir == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "categoryid" => grid.SortDir == "asc" ? query.OrderBy(p => p.TagCategoryId) : query.OrderByDescending(p => p.TagCategoryId),
                "active" => grid.SortDir == "asc" ? query.OrderBy(p => p.IsActive) : query.OrderByDescending(p => p.IsActive),
                _ => query.OrderByDescending(p => p.Id)
            };

            var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await query.ApplyPaging(grid)
                .Select(p => new { p.Id, p.Name, p.TagCategoryId, p.IsActive })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var rows = items.Select(x => (IReadOnlyList<string?>)new string?[]
            {
                x.Id.ToString(), x.Name, x.TagCategoryId.ToString(), x.IsActive ? "Evet" : "Hayır"
            });
            return EntityList(BuildList("Etiketler", "Tags", new[] { "Id", "Name", "CategoryId", "IsActive" }, rows, grid.Search,
                totalCount: total, grid: grid, ajaxDeleteAction: "DeleteTagGridItem"));
        }
        catch (Exception ex)
        {
            return EntityList(BuildList("Etiketler", "Tags", new[] { "Id", "Name", "CategoryId", "IsActive" }, Array.Empty<IReadOnlyList<string?>>(), grid.Search, ex.Message, grid: grid));
        }
    }

    [HttpGet]
    public async Task<IActionResult> SaveOrEdit(int id = 0, CancellationToken cancellationToken = default)
    {
        AdminEditViewModel model;
        if (id <= 0)
        {
            model = new AdminEditViewModel
            {
                Title = "Etiketler — Yeni",
                ControllerName = "Tags",
                EditProfile = "tag",
                IsActive = true
            };
        }
        else
        {
            var entity = await _db.Tags.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
            if (entity is null) return NotFound();

            model = new AdminEditViewModel
            {
                Title = "Etiketler — Düzenle",
                ControllerName = "Tags",
                EditProfile = "tag",
                Id = entity.Id,
                Name = entity.Name ?? string.Empty,
                TagCategoryId = entity.TagCategoryId,
                IsActive = entity.IsActive,
                Position = entity.Position
            };
        }

        model.TagCategories = await _db.TagCategories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.TagCategoryId))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOrEdit(AdminEditViewModel model, CancellationToken cancellationToken)
    {
        model.EditProfile = "tag";
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Name is required");
            model.TagCategories = await _db.TagCategories.AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.TagCategoryId))
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", model);
        }

        if (model.Id > 0)
        {
            var entity = await _db.Tags.FirstOrDefaultAsync(p => p.Id == model.Id, cancellationToken).ConfigureAwait(false);
            if (entity is null) return NotFound();
            entity.Name = model.Name.Trim();
            if (model.TagCategoryId.HasValue) entity.TagCategoryId = model.TagCategoryId.Value;
            entity.IsActive = model.IsActive;
            entity.Position = model.Position;
            entity.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            var categoryId = model.TagCategoryId
                ?? await _db.TagCategories.AsNoTracking().Select(c => c.Id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            _db.Tags.Add(new Tag
            {
                Name = model.Name.Trim(),
                TagCategoryId = categoryId,
                IsActive = model.IsActive,
                Position = model.Position,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                Lang = SiteOptions.MainLanguage
            });
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Kaydedildi");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.Tags.FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();
        entity.IsActive = false;
        entity.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Etiket pasifleştirildi");
        return RedirectToAction(nameof(Index));
    }
}
