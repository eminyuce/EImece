using EImece.Domain.Core.Services;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class InfoController : BaseController
{
    private const string InfoPrefix = "info-";
    private readonly IStorefrontService _storefront;

    public InfoController(IOptions<EImeceOptions> siteOptions, IStorefrontService storefront)
        : base(siteOptions)
    {
        _storefront = storefront;
    }

    public async Task<IActionResult> Index(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        try
        {
            var menuLink = InfoPrefix + id.Trim();
            var menu = await _storefront.GetMenuPageByLinkAsync(menuLink, SiteOptions.MainLanguage, cancellationToken).ConfigureAwait(false);
            if (menu is null)
            {
                return NotFound();
            }

            return View(new InfoPageViewModel
            {
                Id = menu.Id,
                Name = menu.Name,
                Description = menu.Description,
                InfoKey = id
            });
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
