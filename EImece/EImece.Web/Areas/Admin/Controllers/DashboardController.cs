using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class DashboardController : BaseAdminController
{
    public DashboardController(IOptions<EImeceOptions> siteOptions) : base(siteOptions) { }

    public IActionResult Index()
        => AdminPlaceholder("Dashboard", "Admin dashboard shell — widgets/reports migrate with Admin views (Phase 7).");
}
