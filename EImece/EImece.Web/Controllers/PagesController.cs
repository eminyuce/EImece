using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class PagesController : BaseController
{
    public PagesController(IOptions<EImeceOptions> siteOptions) : base(siteOptions) { }

    public IActionResult Detail(string? id)
        => Placeholder("Page", $"CMS page /i/{id}", new { id });
}
