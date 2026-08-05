using EImece.Domain.Core.Services;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class PagesController : BaseController
{
    private readonly IStorefrontService _storefront;

    public PagesController(IOptions<EImeceOptions> siteOptions, IStorefrontService storefront)
        : base(siteOptions)
    {
        _storefront = storefront;
    }

    public async Task<IActionResult> Detail(string? id, CancellationToken cancellationToken)
    {
        var menuId = SeoIdParser.Parse(id);
        if (menuId <= 0)
        {
            return View(new PageDetailViewModel { Name = "Sayfa", Description = "Geçersiz sayfa kimliği." });
        }

        try
        {
            var menu = await _storefront.GetMenuPageAsync(menuId, cancellationToken).ConfigureAwait(false);
            if (menu is null || !menu.IsActive)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new PageDetailViewModel
            {
                Id = menu.Id,
                Name = menu.Name,
                Description = menu.Description,
                PageTheme = menu.PageTheme,
                MetaKeywords = menu.MetaKeywords
            });
        }
        catch (Exception ex)
        {
            return View(new PageDetailViewModel { Id = menuId, Name = $"Sayfa {menuId}", Description = ex.Message });
        }
    }
}
