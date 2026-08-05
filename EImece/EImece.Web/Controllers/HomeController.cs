using System.Diagnostics;
using EImece.Domain.Core.Services;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;
    private readonly IStorefrontService _storefront;

    public HomeController(ILogger<HomeController> logger, IOptions<EImeceOptions> options, IStorefrontService storefront)
        : base(options)
    {
        _logger = logger;
        _storefront = storefront;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "EImece";
        ViewData["Domain"] = SiteOptions.Domain;
        ViewData["SiteStatus"] = SiteOptions.SiteStatus;

        var model = new HomePageViewModel();
        try
        {
            var lang = SiteOptions.MainLanguage;
            var banners = await _storefront.GetMainPageBannersAsync(lang, cancellationToken).ConfigureAwait(false);
            var products = await _storefront.GetHomeProductsAsync(lang, 12, cancellationToken).ConfigureAwait(false);

            model.Banners = banners.Select(b => new MainPageBannerViewModel
            {
                Id = b.Id,
                Name = b.Name,
                Link = b.Link,
                MainImageId = b.MainImageId
            }).ToList();

            model.Products = products.Select(StorefrontMapping.ToListItem).ToList();
            _logger.LogInformation("Home served with {BannerCount} banners, {ProductCount} products", model.Banners.Count, model.Products.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Home query failed");
            model.Error = ex.Message;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string culture, string? returnUrl = null)
    {
        SetCulture(culture);
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
