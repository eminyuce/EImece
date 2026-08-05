using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class SiteMapController : BaseController
{
    public SiteMapController(IOptions<EImeceOptions> siteOptions) : base(siteOptions) { }

    [HttpGet]
    public IActionResult Index()
    {
        // Minimal sitemap shell — full product/story URLs in Phase 7/8 when services are ready.
        var xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
              <url><loc>/</loc></url>
              <url><loc>/p/arama</loc></url>
            </urlset>
            """;
        return Content(xml, "application/xml");
    }
}
