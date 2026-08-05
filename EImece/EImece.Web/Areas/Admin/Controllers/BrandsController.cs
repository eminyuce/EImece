using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Web.Configuration;
using EImece.Web.Helpers;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class BrandsController : BaseAdminController
{
    private readonly EImeceDbContext _db;

    public BrandsController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db) : base(siteOptions)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 25, string? sort = null, string? sortDir = null, CancellationToken cancellationToken = default)
    {
        var grid = GridQuery(search, page, pageSize, sort, sortDir);
        try
        {
            var query = _db.Brands.AsNoTracking()
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
                .Select(p => new { p.Id, p.Name, p.Position, p.IsActive, p.MainPage })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var rows = items.Select(x => (IReadOnlyList<string?>)new string?[] { x.Id.ToString(), x.Name, x.Position.ToString(), x.IsActive ? "Evet" : "Hayır", x.MainPage ? "Evet" : "Hayır" });
            return EntityList(BuildList("Markalar", "Brands", new[] { "Id", "Name", "Position", "IsActive", "MainPage" }, rows, grid.Search,
                totalCount: total, grid: grid, ajaxDeleteAction: "DeleteBrandGridItem"));
        }
        catch (Exception ex)
        {
            return EntityList(BuildList("Markalar", "Brands", new[] { "Id", "Name", "Position", "IsActive", "MainPage" }, Array.Empty<IReadOnlyList<string?>>(), grid.Search, ex.Message, grid: grid));
        }
    }

    [HttpGet]
    public async Task<IActionResult> SaveOrEdit(int id = 0, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", new AdminEditViewModel
            {
                Title = "Markalar — Yeni",
                ControllerName = "Brands",
                EditProfile = "brand",
                IsActive = true
            });
        }

        var entity = await _db.Brands.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();

        return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", new AdminEditViewModel
        {
            Title = "Markalar — Düzenle",
            ControllerName = "Brands",
            EditProfile = "brand",
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Description = entity.Description,
            MainPage = entity.MainPage,
            IsActive = entity.IsActive,
            Position = entity.Position
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOrEdit(AdminEditViewModel model, CancellationToken cancellationToken)
    {
        model.EditProfile = "brand";
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Name is required");
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", model);
        }

        if (model.Id > 0)
        {
            var entity = await _db.Brands.FirstOrDefaultAsync(p => p.Id == model.Id, cancellationToken).ConfigureAwait(false);
            if (entity is null) return NotFound();
            entity.Name = model.Name.Trim();
            entity.Description = model.Description;
            entity.MainPage = model.MainPage;
            entity.IsActive = model.IsActive;
            entity.Position = model.Position;
            entity.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            _db.Brands.Add(new Brand
            {
                Name = model.Name.Trim(),
                Description = model.Description,
                MainPage = model.MainPage,
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
        var entity = await _db.Brands.FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();
        entity.IsActive = false;
        entity.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Marka pasifleştirildi");
        return RedirectToAction(nameof(Index));
    }
}
