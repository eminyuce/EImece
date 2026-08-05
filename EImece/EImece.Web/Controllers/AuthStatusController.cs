using EImece.Domain.Core.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EImece.Web.Controllers;

/// <summary>
/// Phase 5 proof endpoints for cookie auth + role policies.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthStatusController : ControllerBase
{
    [HttpGet("me")]
    [AllowAnonymous]
    public IActionResult Me()
    {
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            Name = User.Identity?.Name,
            Roles = User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role
                            || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                .Select(c => c.Value)
                .Distinct()
                .ToArray()
        });
    }

    [HttpGet("admin-only")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    public IActionResult AdminOnly() => Ok(new { Status = "OK", Policy = AuthPolicies.AdminOnly });

    [HttpGet("admin-or-editor")]
    [Authorize(Policy = AuthPolicies.AdminOrEditor)]
    public IActionResult AdminOrEditor() => Ok(new { Status = "OK", Policy = AuthPolicies.AdminOrEditor });

    [HttpGet("customer-only")]
    [Authorize(Policy = AuthPolicies.CustomerOnly)]
    public IActionResult CustomerOnly() => Ok(new { Status = "OK", Policy = AuthPolicies.CustomerOnly });
}
