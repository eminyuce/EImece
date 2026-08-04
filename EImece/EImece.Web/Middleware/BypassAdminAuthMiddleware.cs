using System.Security.Claims;
using EImece.Domain.Core.Identity;
using EImece.Web.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace EImece.Web.Middleware;

/// <summary>
/// Debug-only Admin principal for /Admin when EImece:BypassAdminAuth=true
/// (parity with Global.asax Application_PostAuthenticateRequest).
/// </summary>
public sealed class BypassAdminAuthMiddleware
{
    private readonly RequestDelegate _next;

    public BypassAdminAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<EImeceOptions> options)
    {
        var cfg = options.Value;
        if (cfg.BypassAdminAuth
            && context.Request.Path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase)
            && !(context.User.Identity?.IsAuthenticated ?? false))
        {
            var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "bypass-admin"));
            identity.AddClaim(new Claim(ClaimTypes.Name, "BypassAdmin"));
            identity.AddClaim(new Claim(ClaimTypes.Role, RoleNames.Admin));
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context).ConfigureAwait(false);
    }
}
