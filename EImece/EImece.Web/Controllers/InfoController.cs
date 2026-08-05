using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class InfoController : BaseController
{
    public InfoController(IOptions<EImeceOptions> siteOptions) : base(siteOptions) { }

    public IActionResult Index(string? id)
        => Placeholder("Info", $"Info page /info/{id}", new { id });
}
