using Microsoft.AspNetCore.Identity;

namespace EImece.Domain.Core.Identity;

/// <summary>
/// ASP.NET Core Identity user (parity with legacy ApplicationUser FirstName/LastName).
/// Full auth wiring lands in Phase 5; the EF model is registered here for schema readiness.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
