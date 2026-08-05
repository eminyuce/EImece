using EImece.Domain.Core;
using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Domain.Core.Media;
using EImece.Web.Configuration;
using EImece.Web.Helpers;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class SettingsController : BaseAdminController
{
    private readonly EImeceDbContext _db;
    private readonly IMediaFileService _media;

    public SettingsController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db, IMediaFileService media)
        : base(siteOptions)
    {
        _db = db;
        _media = media;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 25, string? sort = null, string? sortDir = null, CancellationToken cancellationToken = default)
    {
        var grid = GridQuery(search, page, pageSize, sort, sortDir);
        try
        {
            var query = _db.Settings.AsNoTracking()
                .Where(p => string.IsNullOrWhiteSpace(grid.Search) || p.Name.Contains(grid.Search));

            query = (grid.Sort?.ToLowerInvariant()) switch
            {
                "name" => grid.SortDir == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "value" => grid.SortDir == "asc" ? query.OrderBy(p => p.SettingValue) : query.OrderByDescending(p => p.SettingValue),
                "active" => grid.SortDir == "asc" ? query.OrderBy(p => p.IsActive) : query.OrderByDescending(p => p.IsActive),
                _ => query.OrderBy(p => p.Name)
            };

            var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await query.ApplyPaging(grid)
                .Select(p => new { p.Id, p.Name, p.SettingValue, p.IsActive })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var rows = items.Select(x => (IReadOnlyList<string?>)new string?[]
            {
                x.Id.ToString(), x.Name, x.SettingValue, x.IsActive ? "Evet" : "Hayır"
            });
            return EntityList(BuildList("Ayarlar", "Settings", new[] { "Id", "Name", "Value", "Active" }, rows, grid.Search,
                totalCount: total, grid: grid, ajaxDeleteAction: "DeleteSettingGridItem"));
        }
        catch (Exception ex)
        {
            return EntityList(BuildList("Ayarlar", "Settings", new[] { "Id", "Name", "Value", "Active" }, Array.Empty<IReadOnlyList<string?>>(), grid.Search, ex.Message, grid: grid));
        }
    }

    [HttpGet]
    public async Task<IActionResult> AddWebSiteLogo(CancellationToken cancellationToken)
    {
        var setting = await _db.Settings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.SettingKey == SiteConstants.WebSiteLogo, cancellationToken)
            .ConfigureAwait(false);
        return RedirectToAction(nameof(WebSiteLogo), new { id = setting?.Id ?? 0 });
    }

    [HttpGet]
    public async Task<IActionResult> WebSiteLogo(int id = 0, CancellationToken cancellationToken = default)
    {
        Setting? setting = null;
        if (id > 0)
        {
            setting = await _db.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            setting = await _db.Settings.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SettingKey == SiteConstants.WebSiteLogo, cancellationToken)
                .ConfigureAwait(false);
        }

        var logoUrl = setting?.SettingValue;
        if (!string.IsNullOrEmpty(logoUrl) && !logoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            logoUrl = _media.UrlBase.TrimEnd('/') + "/images/" + logoUrl.TrimStart('/');
        }

        return View(new WebSiteLogoViewModel
        {
            Id = setting?.Id ?? 0,
            SettingValue = setting?.SettingValue,
            CurrentLogoUrl = logoUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadWebSiteLogo(int id, IFormFile? postedImage, CancellationToken cancellationToken)
    {
        if (postedImage is null || postedImage.Length == 0)
        {
            SetTempStatus("Logo resmi seçiniz", isError: true);
            return RedirectToAction(nameof(WebSiteLogo), new { id });
        }

        _media.EnsureDirectories();
        var safeName = Path.GetFileName(postedImage.FileName);
        var storedName = $"logo-{DateTime.UtcNow:yyyyMMddHHmmss}-{safeName}";
        var relativePath = Path.Combine("images", storedName).Replace('\\', '/');

        await using var ms = new MemoryStream();
        await postedImage.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        await _media.WriteAsync(relativePath, ms.ToArray(), cancellationToken).ConfigureAwait(false);

        Setting setting;
        if (id > 0)
        {
            setting = await _db.Settings.FirstOrDefaultAsync(s => s.Id == id, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Setting not found");
        }
        else
        {
            setting = await _db.Settings.FirstOrDefaultAsync(s => s.SettingKey == SiteConstants.WebSiteLogo, cancellationToken).ConfigureAwait(false)
                ?? new Setting
                {
                    Name = SiteConstants.WebSiteLogo,
                    SettingKey = SiteConstants.WebSiteLogo,
                    IsActive = true,
                    Position = 1,
                    Lang = SiteOptions.MainLanguage,
                    CreatedDate = DateTime.UtcNow
                };
            if (setting.Id == 0) _db.Settings.Add(setting);
        }

        setting.SettingValue = storedName;
        setting.Name = SiteConstants.WebSiteLogo;
        setting.SettingKey = SiteConstants.WebSiteLogo;
        setting.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Logo kaydedildi");
        return RedirectToAction(nameof(WebSiteLogo), new { id = setting.Id });
    }

    [HttpGet]
    public async Task<IActionResult> SaveOrEdit(int id = 0, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", new AdminEditViewModel
            {
                Title = "Ayar — Yeni",
                ControllerName = "Settings",
                EditProfile = "setting",
                IsActive = true
            });
        }

        var entity = await _db.Settings.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();

        return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", new AdminEditViewModel
        {
            Title = "Ayar — " + entity.Name,
            ControllerName = "Settings",
            EditProfile = "setting",
            Id = entity.Id,
            Name = entity.Name,
            SettingKey = entity.SettingKey,
            SettingValue = entity.SettingValue,
            IsActive = entity.IsActive,
            Position = entity.Position
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOrEdit(AdminEditViewModel model, CancellationToken cancellationToken)
    {
        model.EditProfile = "setting";
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Name is required");
            return View("~/Areas/Admin/Views/Shared/EntityEdit.cshtml", model);
        }

        Setting entity;
        if (model.Id > 0)
        {
            entity = await _db.Settings.FirstOrDefaultAsync(p => p.Id == model.Id, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Not found");
        }
        else
        {
            entity = new Setting
            {
                CreatedDate = DateTime.UtcNow,
                Lang = SiteOptions.MainLanguage
            };
            _db.Settings.Add(entity);
        }

        entity.Name = model.Name.Trim();
        entity.SettingKey = model.SettingKey?.Trim() ?? model.Name.Trim();
        entity.SettingValue = model.SettingValue;
        entity.IsActive = model.IsActive;
        entity.Position = model.Position;
        entity.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Ayar kaydedildi");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.Settings.FirstOrDefaultAsync(p => p.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();
        entity.IsActive = false;
        entity.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Ayar pasifleştirildi");
        return RedirectToAction(nameof(Index));
    }
}
