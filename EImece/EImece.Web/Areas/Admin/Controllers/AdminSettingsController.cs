using EImece.Domain.Core.Data;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class AdminSettingsController : BaseAdminController
{
    private readonly EImeceDbContext _db;

    public AdminSettingsController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db) : base(siteOptions)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var settings = await _db.Settings.AsNoTracking()
            .OrderBy(s => s.Name)
            .Take(50)
            .Select(s => new SettingRow
            {
                Id = s.Id,
                Name = s.Name,
                SettingKey = s.SettingKey,
                SettingValue = s.SettingValue,
                IsActive = s.IsActive
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return View(new AdminSettingsViewModel
        {
            BypassAdminAuth = SiteOptions.BypassAdminAuth,
            Domain = SiteOptions.Domain,
            ApplicationLanguages = SiteOptions.ApplicationLanguages,
            MainLanguage = SiteOptions.MainLanguage,
            KeySettings = settings
        });
    }

    [HttpGet]
    public IActionResult SystemSettings()
        => View(new SystemSettingsViewModel
        {
            SiteStatus = SiteOptions.SiteStatus,
            IsSiteUnderConstruction = SiteOptions.IsSiteUnderConstruction,
            MainLanguage = SiteOptions.MainLanguage,
            BypassAdminAuth = SiteOptions.BypassAdminAuth,
            DatabaseCommandTimeoutSeconds = SiteOptions.DatabaseCommandTimeoutSeconds
        });
}
