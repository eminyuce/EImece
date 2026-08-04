namespace EImece.Domain.Core.Identity;

/// <summary>Role name parity with legacy EImece.Domain.Constants.</summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string NormalUser = "NormalUser";
    public const string Customer = "Customer";
}

/// <summary>Authorization policy names for ASP.NET Core.</summary>
public static class AuthPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string AdminOrEditor = "AdminOrEditor";
    public const string CustomerOnly = "CustomerOnly";
}
