using System.Diagnostics;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly EImeceOptions _options;

    public HomeController(ILogger<HomeController> logger, IOptions<EImeceOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "EImece Core Host";
        ViewData["Phase"] = "Phase 3 — Domain & data layer (EF Core)";
        ViewData["Domain"] = _options.Domain;
        ViewData["SiteStatus"] = _options.SiteStatus;
        ViewData["SampleResource"] = Resources.Resource.ResourceManager != null
            ? "Resources assembly loaded"
            : "Resources assembly missing";
        ViewData["Orm"] = "Entity Framework Core 8";
        _logger.LogInformation("EImece.Web home served (domain={Domain})", _options.Domain);
        return View();
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
