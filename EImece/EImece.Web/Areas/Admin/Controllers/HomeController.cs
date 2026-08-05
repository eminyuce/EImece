using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class HomeController : BaseAdminController
{
    public HomeController(IOptions<EImeceOptions> options) : base(options) { }

    public IActionResult Index()
    {
        ViewData["Title"] = "Admin";
        ViewData["Message"] =
            $"Admin home · user={User.Identity?.Name ?? "(anonymous)"} · BypassAdminAuth={SiteOptions.BypassAdminAuth}";
        return View("~/Areas/Admin/Views/Shared/Placeholder.cshtml");
    }
}
