using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class StoriesController : BaseController
{
    public StoriesController(IOptions<EImeceOptions> siteOptions) : base(siteOptions) { }

    public IActionResult Detail(string categoryName, string? id)
        => Placeholder("Story", $"Story detail /s/{categoryName}/{id}", new { categoryName, id });

    public IActionResult Tag(string? id)
        => Placeholder("Story tag", $"Story tag /s/t/{id}", new { id });

    public IActionResult Categories(string? id)
        => Placeholder("Story categories", $"Story categories /s/categories/{id}", new { id });
}
