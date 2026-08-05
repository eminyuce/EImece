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

public sealed class StoriesController : BaseAdminController
{
    private readonly EImeceDbContext _db;

    public StoriesController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db) : base(siteOptions)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 25, string? sort = null, string? sortDir = null, CancellationToken cancellationToken = default)
    {
        var grid = GridQuery(search, page, pageSize, sort, sortDir);
        try
        {
            var query = _db.Stories.AsNoTracking()
                .Where(p => string.IsNullOrWhiteSpace(grid.Search) || (p.Name != null && p.Name.Contains(grid.Search)));

            query = (grid.Sort?.ToLowerInvariant()) switch
            {
                "name" => grid.SortDir == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "active" => grid.SortDir == "asc" ? query.OrderBy(p => p.IsActive) : query.OrderByDescending(p => p.IsActive),
                "mainpage" => grid.SortDir == "asc" ? query.OrderBy(p => p.MainPage) : query.OrderByDescending(p => p.MainPage),
                _ => query.OrderByDescending(p => p.Id)
            };

            var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await query.ApplyPaging(grid)
                .Select(p => new { p.Id, p.Name, p.IsActive, p.MainPage })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var rows = items.Select(x => (IReadOnlyList<string?>)new string?[] { x.Id.ToString(), x.Name, x.IsActive ? "Evet" : "Hayır", x.MainPage ? "Evet" : "Hayır" });
            return EntityList(BuildList("Hikayeler", "Stories", new[] { "Id", "Name", "IsActive", "MainPage" }, rows, grid.Search,
                totalCount: total, grid: grid, ajaxDeleteAction: "DeleteStoryGridItem"));
        }
        catch (Exception ex)
        {
            return EntityList(BuildList("Hikayeler", "Stories", new[] { "Id", "Name", "IsActive", "MainPage" }, Array.Empty<IReadOnlyList<string?>>(), grid.Search, ex.Message, grid: grid));
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
                Title = "Hikayeler — Yeni",
                ControllerName = "Stories",
                EditProfile = "story",
                IsActive = true
            };
        }
        else
        {
            var entity = await _db.Stories.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
            if (entity is null) return NotFound();

            model = new AdminEditViewModel
            {
                Title = "Hikayeler — Düzenle",
                ControllerName = "Stories",
                EditProfile = "story",
                Id = entity.Id,
                Name = entity.Name ?? string.Empty,
                ShortDescription = entity.ShortDescription,
                Description = entity.Description,
                StoryCategoryId = entity.StoryCategoryId,
                MainPage = entity.MainPage,
                IsActive = entity.IsActive,
                Position = entity.Position
            };
        }

        model.StoryCategories = await _db.StoryCategories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.StoryCategoryId))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOrEdit(AdminEditViewModel model, CancellationToken cancellationToken)
    {
        model.EditProfile = "story";
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Name is required");
            model.StoryCategories = await _db.StoryCategories.AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.StoryCategoryId))
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", model);
        }

        if (model.Id > 0)
        {
            var entity = await _db.Stories.FirstOrDefaultAsync(p => p.Id == model.Id, cancellationToken).ConfigureAwait(false);
            if (entity is null) return NotFound();
            ApplyStory(entity, model);
            entity.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            var categoryId = model.StoryCategoryId
                ?? await _db.StoryCategories.AsNoTracking().Select(c => c.Id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            var entity = new Story
            {
                Name = model.Name.Trim(),
                StoryCategoryId = categoryId,
                CreatedDate = DateTime.UtcNow,
                Lang = SiteOptions.MainLanguage
            };
            ApplyStory(entity, model);
            entity.UpdatedDate = DateTime.UtcNow;
            _db.Stories.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Kaydedildi");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.Stories.FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();
        entity.IsActive = false;
        entity.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Hikaye pasifleştirildi");
        return RedirectToAction(nameof(Index));
    }

    private static void ApplyStory(Story entity, AdminEditViewModel model)
    {
        entity.Name = model.Name.Trim();
        entity.ShortDescription = model.ShortDescription;
        entity.Description = model.Description;
        entity.MainPage = model.MainPage;
        entity.IsActive = model.IsActive;
        entity.Position = model.Position;
        if (model.StoryCategoryId.HasValue) entity.StoryCategoryId = model.StoryCategoryId.Value;
    }
}
