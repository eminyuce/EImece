namespace EImece.Domain.Core.Configuration;

public sealed class CacheOptions
{
    public const string SectionName = "Cache";

    public bool IsCacheActive { get; set; } = true;
    public int TinySeconds { get; set; } = 1;
    public int ShortSeconds { get; set; } = 10;
    public int MediumSeconds { get; set; } = 300;
    public int LongSeconds { get; set; } = 1800;
    public int VeryLongSeconds { get; set; } = 180000;
}

public sealed class CaptchaOptions
{
    public const string SectionName = "Captcha";

    /// <summary>Legacy | Recaptcha | None</summary>
    public string Provider { get; set; } = "Legacy";
    public bool RecaptchaEnabled { get; set; }
    public string RecaptchaSiteKey { get; set; } = string.Empty;
    public string RecaptchaSecretKey { get; set; } = string.Empty;
}

public sealed class MediaOptions
{
    public const string SectionName = "Media";

    /// <summary>Relative to content root, e.g. "wwwroot/media" (legacy ~/media).</summary>
    public string RootRelativePath { get; set; } = "wwwroot/media";

    /// <summary>Optional absolute root (IIS shared media). When set, overrides RootRelativePath.</summary>
    public string? AbsoluteRootPath { get; set; }

    public string ImagesSubPath { get; set; } = "images";
    public string TempSubPath { get; set; } = "tempFiles";
    public bool IsImageFullSrcUnderMediaFolder { get; set; } = true;
    public string UrlBase { get; set; } = "/media/images/";
}

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public bool EnableRequestLogging { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
    public bool EnableEfSqlLogging { get; set; }
    public bool ExposeDetailedErrors { get; set; }
}

public sealed class HttpClientResilienceOptions
{
    public const string SectionName = "HttpClient";

    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public int CircuitBreakerFailures { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}

public sealed class QuartzOptions
{
    public const string SectionName = "Quartz";

    /// <summary>Default false — matches legacy Web.config and Phase 2 decision.</summary>
    public bool IsEnabled { get; set; }
    public string HelloJobCron { get; set; } = "0 0 0/24 * * ?";
}

/// <summary>SMTP settings for MailKitEmailSender (Phase 8). Empty Host → log-only sink.</summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public bool IsEnabled { get; set; } = true;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string FromAddress { get; set; } = string.Empty;
    public string FromDisplayName { get; set; } = "EImece";

    public bool CanSend =>
        IsEnabled
        && !string.IsNullOrWhiteSpace(Host)
        && !string.IsNullOrWhiteSpace(FromAddress);
}

