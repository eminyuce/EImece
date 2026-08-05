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
        ViewData["Title"] = "Yapım aşamasında";
        ViewData["IsUnderConstruction"] = _options.IsSiteUnderConstruction;
        return View();
    }
}
