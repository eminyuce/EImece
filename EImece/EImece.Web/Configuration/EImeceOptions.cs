namespace EImece.Web.Configuration;

/// <summary>
/// Site-level Options formerly stored in Web.config appSettings.
/// Cache/Captcha/Media/Quartz/Smtp/HttpClient live in dedicated Options classes (Phase 4).
/// </summary>
public sealed class EImeceOptions
{
    public const string SectionName = "EImece";

    public string Domain { get; set; } = "localhost";
    public bool UseSsl { get; set; }
    public string SiteStatus { get; set; } = "live";
    public bool IsSiteUnderConstruction { get; set; }
    /// <summary>Mirrored into Quartz:IsEnabled when set.</summary>
    public bool QuartzSchedulerIsEnabled { get; set; }
    public string ApplicationLanguages { get; set; } = "tr-TR";
    public int MainLanguage { get; set; } = 1;
    public bool AdminLoginEnabled { get; set; } = true;
    public bool BypassAdminAuth { get; set; }
    public int DatabaseCommandTimeoutSeconds { get; set; } = 120;
}
