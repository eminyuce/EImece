using EImece.Domain.Core.Identity;
using EImece.Web.Configuration;
using EImece.Web.Infrastructure.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

/// <summary>
/// Admin base — role policy parity with legacy [AuthorizeRoles(Admin, NormalUser)].
/// Fat [Inject] service graph deferred until Domain.Core services exist.
/// </summary>
[Area("Admin")]
[Authorize(Policy = AuthPolicies.AdminOrEditor)]
public abstract class BaseAdminController : Controller
{
    protected EImeceOptions SiteOptions { get; }

    protected BaseAdminController(IOptions<EImeceOptions> siteOptions)
    {
        SiteOptions = siteOptions.Value;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewData["AdminUser"] = User.Identity?.Name;
        ViewData["Phase"] = "Phase 6 — Application layer";
        base.OnActionExecuting(context);
    }

    protected void SetAdminCulture(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return;
        }

        Response.Cookies.Append(
            RouteConstants.AdminCultureCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, Path = "/" });
    }

    protected void SetTempStatus(string message, bool isError = false)
    {
        TempData[isError ? "Error" : "Status"] = message;
    }

    protected IActionResult AdminPlaceholder(string title, string message)
    {
        ViewData["Title"] = title;
        ViewData["Message"] = message;
        return View("~/Areas/Admin/Views/Shared/Placeholder.cshtml");
    }
}
