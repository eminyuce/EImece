using EImece.Domain.Core.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EImece.Web.Areas.Customers.Controllers;

[Area("Customers")]
[Authorize(Policy = AuthPolicies.CustomerOnly)]
public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Customer account";
        ViewData["Message"] = $"Customer portal shell · user={User.Identity?.Name}. Orders/profile UI migrates in Phase 7.";
        return View("~/Areas/Customers/Views/Home/Index.cshtml");
    }
}
