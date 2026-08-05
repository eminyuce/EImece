using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Web.Configuration;
using EImece.Web.Helpers;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class MenusController : BaseAdminController
{
    private readonly EImeceDbContext _db;

    public MenusController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db) : base(siteOptions)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 25, string? sort = null, string? sortDir = null, CancellationToken cancellationToken = default)
    {
        var grid = GridQuery(search, page, pageSize, sort, sortDir);
        try
        {
            var query = _db.Menus.AsNoTracking()
                .Where(p => string.IsNullOrWhiteSpace(grid.Search) || (p.Name != null && p.Name.Contains(grid.Search)));

            query = (grid.Sort?.ToLowerInvariant()) switch
            {
                "name" => grid.SortDir == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "active" => grid.SortDir == "asc" ? query.OrderBy(p => p.IsActive) : query.OrderByDescending(p => p.IsActive),
                "link" => grid.SortDir == "asc" ? query.OrderBy(p => p.Link) : query.OrderByDescending(p => p.Link),
                _ => query.OrderByDescending(p => p.Id)
            };

            var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await query.ApplyPaging(grid)
                .Select(p => new { p.Id, p.Name, p.IsActive, p.Link })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var rows = items.Select(x => (IReadOnlyList<string?>)new string?[] { x.Id.ToString(), x.Name, x.IsActive ? "Evet" : "Hayır", x.Link });
            return EntityList(BuildList("Menüler", "Menus", new[] { "Id", "Name", "IsActive", "Link" }, rows, grid.Search,
                totalCount: total, grid: grid, ajaxDeleteAction: "DeleteMenusGridItem"));
        }
        catch (Exception ex)
        {
            return EntityList(BuildList("Menüler", "Menus", new[] { "Id", "Name", "IsActive", "Link" }, Array.Empty<IReadOnlyList<string?>>(), grid.Search, ex.Message, grid: grid));
        }
    }

    [HttpGet]
    public async Task<IActionResult> SaveOrEdit(int id = 0, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", new AdminEditViewModel
            {
                Title = "Menüler — Yeni",
                ControllerName = "Menus",
                EditProfile = "menu",
                IsActive = true,
                LinkIsActive = true
            });
        }

        var entity = await _db.Menus.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();

        return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", new AdminEditViewModel
        {
            Title = "Menüler — Düzenle",
            ControllerName = "Menus",
            EditProfile = "menu",
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Link = entity.Link,
            MenuLink = entity.MenuLink,
            ParentId = entity.ParentId,
            MainPage = entity.MainPage,
            LinkIsActive = entity.LinkIsActive,
            Description = entity.Description,
            IsActive = entity.IsActive,
            Position = entity.Position
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOrEdit(AdminEditViewModel model, CancellationToken cancellationToken)
    {
        model.EditProfile = "menu";
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Name is required");
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", model);
        }

        if (model.Id > 0)
        {
            var entity = await _db.Menus.FirstOrDefaultAsync(p => p.Id == model.Id, cancellationToken).ConfigureAwait(false);
            if (entity is null) return NotFound();
            ApplyMenu(entity, model);
            entity.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            var entity = new Menu
            {
                Name = model.Name.Trim(),
                CreatedDate = DateTime.UtcNow,
                Lang = SiteOptions.MainLanguage
            };
            ApplyMenu(entity, model);
            entity.UpdatedDate = DateTime.UtcNow;
            _db.Menus.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Kaydedildi");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.Menus.FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();
        entity.IsActive = false;
        entity.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Menü pasifleştirildi");
        return RedirectToAction(nameof(Index));
    }

    private static void ApplyMenu(Menu entity, AdminEditViewModel model)
    {
        entity.Name = model.Name.Trim();
        entity.Link = model.Link;
        entity.MenuLink = model.MenuLink;
        entity.ParentId = model.ParentId;
        entity.MainPage = model.MainPage;
        entity.LinkIsActive = model.LinkIsActive;
        entity.Description = model.Description;
        entity.IsActive = model.IsActive;
        entity.Position = model.Position;
    }
}
