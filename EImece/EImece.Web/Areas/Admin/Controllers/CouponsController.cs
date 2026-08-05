using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Web.Configuration;
using EImece.Web.Helpers;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class CouponsController : BaseAdminController
{
    private readonly EImeceDbContext _db;

    public CouponsController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db) : base(siteOptions)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 25, string? sort = null, string? sortDir = null, CancellationToken cancellationToken = default)
    {
        var grid = GridQuery(search, page, pageSize, sort, sortDir);
        try
        {
            var query = _db.Coupons.AsNoTracking()
                .Where(p => string.IsNullOrWhiteSpace(grid.Search) || p.Name.Contains(grid.Search) || p.Code.Contains(grid.Search));

            query = (grid.Sort?.ToLowerInvariant()) switch
            {
                "name" => grid.SortDir == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "code" => grid.SortDir == "asc" ? query.OrderBy(p => p.Code) : query.OrderByDescending(p => p.Code),
                "active" => grid.SortDir == "asc" ? query.OrderBy(p => p.IsActive) : query.OrderByDescending(p => p.IsActive),
                _ => query.OrderByDescending(p => p.Id)
            };

            var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await query.ApplyPaging(grid)
                .Select(p => new { p.Id, p.Name, p.Code, p.IsActive })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var rows = items.Select(x => (IReadOnlyList<string?>)new string?[] { x.Id.ToString(), x.Name, x.Code, x.IsActive ? "Evet" : "Hayır" });
            return EntityList(BuildList("Kuponlar", "Coupons", new[] { "Id", "Name", "Code", "IsActive" }, rows, grid.Search,
                totalCount: total, grid: grid, ajaxDeleteAction: "DeleteCouponsGridItem"));
        }
        catch (Exception ex)
        {
            return EntityList(BuildList("Kuponlar", "Coupons", new[] { "Id", "Name", "Code", "IsActive" }, Array.Empty<IReadOnlyList<string?>>(), grid.Search, ex.Message, grid: grid));
        }
    }

    [HttpGet]
    public async Task<IActionResult> SaveOrEdit(int id = 0, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", new AdminEditViewModel
            {
                Title = "Kuponlar — Yeni",
                ControllerName = "Coupons",
                EditProfile = "coupon",
                IsActive = true,
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddMonths(1)
            });
        }

        var entity = await _db.Coupons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();

        return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", new AdminEditViewModel
        {
            Title = "Kuponlar — Düzenle",
            ControllerName = "Coupons",
            EditProfile = "coupon",
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            DiscountPercentage = entity.DiscountPercentage,
            CouponDiscountAmount = entity.Discount,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            IsActive = entity.IsActive,
            Position = entity.Position
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOrEdit(AdminEditViewModel model, CancellationToken cancellationToken)
    {
        model.EditProfile = "coupon";
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Name is required");
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", model);
        }

        if (model.Id > 0)
        {
            var entity = await _db.Coupons.FirstOrDefaultAsync(p => p.Id == model.Id, cancellationToken).ConfigureAwait(false);
            if (entity is null) return NotFound();
            ApplyCoupon(entity, model);
            entity.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            var entity = new Coupon
            {
                Name = model.Name.Trim(),
                CreatedDate = DateTime.UtcNow,
                Lang = SiteOptions.MainLanguage
            };
            ApplyCoupon(entity, model);
            entity.UpdatedDate = DateTime.UtcNow;
            _db.Coupons.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Kaydedildi");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.Coupons.FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();
        entity.IsActive = false;
        entity.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Kupon pasifleştirildi");
        return RedirectToAction(nameof(Index));
    }

    private static void ApplyCoupon(Coupon entity, AdminEditViewModel model)
    {
        entity.Name = model.Name.Trim();
        entity.Code = model.Code?.Trim() ?? entity.Code;
        entity.DiscountPercentage = model.DiscountPercentage;
        entity.Discount = model.CouponDiscountAmount;
        entity.StartDate = model.StartDate ?? entity.StartDate;
        entity.EndDate = model.EndDate ?? entity.EndDate;
        entity.IsActive = model.IsActive;
        entity.Position = model.Position;
    }
}
