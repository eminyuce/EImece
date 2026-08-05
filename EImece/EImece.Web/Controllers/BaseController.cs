using System.Globalization;
using EImece.Web.Configuration;
using EImece.Web.Filters;
using EImece.Web.Infrastructure.Routing;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

/// <summary>
/// Storefront base controller — culture + common ViewData (legacy BaseController parity, ctor DI).
/// Full ISettingService wiring arrives with Domain.Core services.
/// </summary>
[UnderConstructionFilter]
public abstract class BaseController : Controller
{
    protected EImeceOptions SiteOptions { get; }

    protected BaseController(IOptions<EImeceOptions> siteOptions)
    {
        SiteOptions = siteOptions.Value;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewData["IsProductPriceEnable"] = true; // default until settings service migrates
        ViewData["SiteStatus"] = SiteOptions.SiteStatus;
        ViewData["Phase"] = "Phase 6 — Application layer";
        base.OnActionExecuting(context);
    }

    protected void SetCulture(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return;
        }

        try
        {
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            Response.Cookies.Append(
                RouteConstants.CultureCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultureInfo)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, Path = "/" });
            Response.Cookies.Append(
                RouteConstants.LanguageCookieName,
                cultureInfo.Name,
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, Path = "/" });
        }
        catch (CultureNotFoundException)
        {
            // ignore invalid culture values
        }
    }

    protected IActionResult Placeholder(string title, string message, object? routeValues = null)
    {
        ViewData["Title"] = title;
        ViewData["Message"] = message;
        ViewData["RouteValues"] = routeValues;
        return View("~/Views/Shared/Placeholder.cshtml");
    }
}
