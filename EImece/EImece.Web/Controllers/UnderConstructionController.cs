using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class UnderConstructionController : Controller
{
    private readonly EImeceOptions _options;

    public UnderConstructionController(IOptions<EImeceOptions> options)
    {
        _options = options.Value;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Under construction";
        ViewData["Message"] = _options.IsSiteUnderConstruction
            ? "This site is temporarily under construction."
            : "Under construction page (flag currently off).";
        return View("~/Views/Shared/Placeholder.cshtml");
    }
}
