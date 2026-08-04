using EImece.Domain.Core.Identity;
using EImece.Web.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = AuthPolicies.AdminOrEditor)]
public sealed class HomeController : Controller
{
    private readonly EImeceOptions _options;

    public HomeController(IOptions<EImeceOptions> options)
    {
        _options = options.Value;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Admin";
        ViewData["User"] = User.Identity?.Name ?? "(anonymous bypass)";
        ViewData["Bypass"] = _options.BypassAdminAuth;
        return View();
    }
}
