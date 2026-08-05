using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class RobotController : BaseController
{
    public RobotController(IOptions<EImeceOptions> siteOptions) : base(siteOptions) { }

    [HttpGet]
    public IActionResult RobotsText()
    {
        var body = """
            User-agent: *
            Allow: /
            Sitemap: /sitemap.xml
            """;
        return Content(body, "text/plain");
    }
}
