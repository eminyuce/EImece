namespace EImece.Web.Configuration;

/// <summary>
/// Baseline Options mapping for settings formerly stored in Web.config appSettings.
/// Full Options coverage lands in Phase 4; this stub establishes the pattern.
/// </summary>
public sealed class EImeceOptions
{
    public const string SectionName = "EImece";

    public string Domain { get; set; } = "localhost";
    public bool UseSsl { get; set; }
    public string SiteStatus { get; set; } = "live";
    public bool IsSiteUnderConstruction { get; set; }
    public bool QuartzSchedulerIsEnabled { get; set; }
    public bool IsCacheActive { get; set; } = true;
    public string ApplicationLanguages { get; set; } = "tr-TR";
    public int MainLanguage { get; set; } = 1;
    public bool AdminLoginEnabled { get; set; } = true;
    public bool BypassAdminAuth { get; set; }
    public int DatabaseCommandTimeoutSeconds { get; set; } = 120;
}

public sealed class IyzicoOptions
{
    public const string SectionName = "Iyzico";

    public string BaseUrl { get; set; } = "https://sandbox-api.iyzipay.com";
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string EnabledInstallments { get; set; } = "1,2,4,6,9";
}
