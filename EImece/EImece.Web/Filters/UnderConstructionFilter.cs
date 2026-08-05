using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace EImece.Web.Filters;

/// <summary>
/// Legacy [UnderConst] parity — redirects storefront to /UnderConstruction when enabled.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class UnderConstructionFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var options = context.HttpContext.RequestServices.GetService<IOptions<EImeceOptions>>()?.Value;
        if (options is null || !options.IsSiteUnderConstruction)
        {
            return;
        }

        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/UnderConstruction", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        context.Result = new RedirectToActionResult("Index", "UnderConstruction", null);
    }
}
