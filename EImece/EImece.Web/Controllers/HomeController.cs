using System.Diagnostics;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, IOptions<EImeceOptions> options)
        : base(options)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "EImece";
        ViewData["Phase"] = "Phase 7 · Presentation";
        ViewData["Domain"] = SiteOptions.Domain;
        ViewData["SiteStatus"] = SiteOptions.SiteStatus;
        _logger.LogInformation("EImece.Web home served (domain={Domain})", SiteOptions.Domain);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string culture, string? returnUrl = null)
    {
        SetCulture(culture);
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
