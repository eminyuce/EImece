using EImece.Domain.Core.Data;
using EImece.Web.Configuration;
using EImece.Web.Helpers;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class SubscribersController : BaseAdminController
{
    private readonly EImeceDbContext _db;

    public SubscribersController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db) : base(siteOptions)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 25, string? sort = null, string? sortDir = null, CancellationToken cancellationToken = default)
    {
        var grid = GridQuery(search, page, pageSize, sort, sortDir);
        try
        {
            var query = _db.Subscribers.AsNoTracking()
                .Where(p => string.IsNullOrWhiteSpace(grid.Search) || (p.Name != null && p.Name.Contains(grid.Search)));

            query = (grid.Sort?.ToLowerInvariant()) switch
            {
                "name" => grid.SortDir == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "active" => grid.SortDir == "asc" ? query.OrderBy(p => p.IsActive) : query.OrderByDescending(p => p.IsActive),
                _ => query.OrderByDescending(p => p.Id)
            };

            var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await query.ApplyPaging(grid)
                .Select(p => new { p.Id, p.Name, p.IsActive })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var rows = items.Select(x => (IReadOnlyList<string?>)new string?[] { x.Id.ToString(), x.Name, x.IsActive ? "Evet" : "Hayır" });
            return EntityList(BuildList("Aboneler", "Subscribers", new[] { "Id", "Name", "IsActive" }, rows, grid.Search,
                totalCount: total, grid: grid));
        }
        catch (Exception ex)
        {
            return EntityList(BuildList("Aboneler", "Subscribers", new[] { "Id", "Name", "IsActive" }, Array.Empty<IReadOnlyList<string?>>(), grid.Search, ex.Message, grid: grid));
        }
    }

    [HttpGet]
    public async Task<IActionResult> SaveOrEdit(int id = 0, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", new AdminEditViewModel
            {
                Title = "Aboneler — Yeni",
                ControllerName = "Subscribers",
                Id = 0,
                Name = string.Empty,
                IsActive = true
            });
        }

        var entity = await _db.Subscribers.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();

        return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", new AdminEditViewModel
        {
            Title = "Aboneler — Düzenle",
            ControllerName = "Subscribers",
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            IsActive = entity.IsActive,
            Position = entity.Position
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOrEdit(AdminEditViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Name is required");
            model.Title = "Aboneler";
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", model);
        }

        if (model.Id > 0)
        {
            var entity = await _db.Subscribers.FirstOrDefaultAsync(p => p.Id == model.Id, cancellationToken).ConfigureAwait(false);
            if (entity is null) return NotFound();
            entity.Name = model.Name.Trim();
            entity.IsActive = model.IsActive;
            entity.Position = model.Position;
            entity.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            var entityType = _db.Subscribers.EntityType.ClrType;
            var entity = Activator.CreateInstance(entityType)!;
            entityType.GetProperty("Name")!.SetValue(entity, model.Name.Trim());
            entityType.GetProperty("IsActive")!.SetValue(entity, model.IsActive);
            entityType.GetProperty("Position")!.SetValue(entity, model.Position);
            entityType.GetProperty("CreatedDate")!.SetValue(entity, DateTime.UtcNow);
            entityType.GetProperty("UpdatedDate")!.SetValue(entity, DateTime.UtcNow);
            entityType.GetProperty("Lang")!.SetValue(entity, SiteOptions.MainLanguage);
            _db.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Kaydedildi");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.Subscribers.FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();
        _db.Subscribers.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Silindi");
        return RedirectToAction(nameof(Index));
    }
}
