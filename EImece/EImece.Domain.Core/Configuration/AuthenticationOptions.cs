namespace EImece.Domain.Core.Configuration;

/// <summary>
/// External OAuth providers — registered only when ClientId/AppId is non-empty (legacy parity).
/// </summary>
public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public CookieAuthOptions Cookie { get; set; } = new();
    public OAuthProviderOptions Google { get; set; } = new();
    public OAuthProviderOptions Facebook { get; set; } = new();
    public OAuthProviderOptions Microsoft { get; set; } = new();
    public OAuthProviderOptions Twitter { get; set; } = new();
}

public sealed class CookieAuthOptions
{
    public string LoginPath { get; set; } = "/Account/Login";
    public string AccessDeniedPath { get; set; } = "/Account/AccessDenied";
    public string LogoutPath { get; set; } = "/Account/Logout";
    public int ExpireDays { get; set; } = 14;
    public bool SlidingExpiration { get; set; } = true;
    public int SecurityStampValidationIntervalMinutes { get; set; } = 30;
}

public sealed class OAuthProviderOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}
