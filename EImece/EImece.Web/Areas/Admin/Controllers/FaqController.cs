using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Web.Configuration;
using EImece.Web.Helpers;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class FaqController : BaseAdminController
{
    private readonly EImeceDbContext _db;

    public FaqController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db) : base(siteOptions)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 25, string? sort = null, string? sortDir = null, CancellationToken cancellationToken = default)
    {
        var grid = GridQuery(search, page, pageSize, sort, sortDir);
        try
        {
            var query = _db.Faqs.AsNoTracking()
                .Where(p => string.IsNullOrWhiteSpace(grid.Search)
                    || p.Name.Contains(grid.Search)
                    || (p.Question != null && p.Question.Contains(grid.Search)));

            query = (grid.Sort?.ToLowerInvariant()) switch
            {
                "soru" => grid.SortDir == "asc" ? query.OrderBy(p => p.Question ?? p.Name) : query.OrderByDescending(p => p.Question ?? p.Name),
                "active" => grid.SortDir == "asc" ? query.OrderBy(p => p.IsActive) : query.OrderByDescending(p => p.IsActive),
                _ => query.OrderByDescending(p => p.Id)
            };

            var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await query.ApplyPaging(grid)
                .Select(p => new { p.Id, p.Name, p.Question, p.IsActive })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var rows = items.Select(x => (IReadOnlyList<string?>)new string?[]
            {
                x.Id.ToString(), x.Question ?? x.Name, x.IsActive ? "Evet" : "Hayır"
            });
            return EntityList(BuildList("SSS", "Faq", new[] { "Id", "Soru", "IsActive" }, rows, grid.Search,
                totalCount: total, grid: grid, ajaxDeleteAction: "DeleteFaqGridItem"));
        }
        catch (Exception ex)
        {
            return EntityList(BuildList("SSS", "Faq", new[] { "Id", "Soru", "IsActive" }, Array.Empty<IReadOnlyList<string?>>(), grid.Search, ex.Message, grid: grid));
        }
    }

    [HttpGet]
    public async Task<IActionResult> SaveOrEdit(int id = 0, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", new AdminEditViewModel
            {
                Title = "SSS — Yeni",
                ControllerName = "Faq",
                EditProfile = "faq",
                IsActive = true
            });
        }

        var entity = await _db.Faqs.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();

        return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", new AdminEditViewModel
        {
            Title = "SSS — Düzenle",
            ControllerName = "Faq",
            EditProfile = "faq",
            Id = entity.Id,
            Name = entity.Question ?? entity.Name,
            Answer = entity.Answer,
            IsActive = entity.IsActive,
            Position = entity.Position
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOrEdit(AdminEditViewModel model, CancellationToken cancellationToken)
    {
        model.EditProfile = "faq";
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Soru gerekli");
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", model);
        }

        if (model.Id > 0)
        {
            var entity = await _db.Faqs.FirstOrDefaultAsync(p => p.Id == model.Id, cancellationToken).ConfigureAwait(false);
            if (entity is null) return NotFound();
            entity.Name = model.Name.Trim();
            entity.Question = model.Name.Trim();
            entity.Answer = model.Answer;
            entity.IsActive = model.IsActive;
            entity.Position = model.Position;
            entity.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            _db.Faqs.Add(new Faq
            {
                Name = model.Name.Trim(),
                Question = model.Name.Trim(),
                Answer = model.Answer,
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
        var entity = await _db.Faqs.FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();
        entity.IsActive = false;
        entity.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("SSS pasifleştirildi");
        return RedirectToAction(nameof(Index));
    }
}
