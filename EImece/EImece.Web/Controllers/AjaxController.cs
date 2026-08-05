using EImece.Domain.Core.Cart;
using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Domain.Core.Services;
using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

public sealed class AjaxController : BaseController
{
    private readonly EImeceDbContext _db;
    private readonly ITurkishRegionService _regions;
    private readonly IShoppingCartService _cart;

    public AjaxController(
        IOptions<EImeceOptions> siteOptions,
        EImeceDbContext db,
        ITurkishRegionService regions,
        IShoppingCartService cart)
        : base(siteOptions)
    {
        _db = db;
        _regions = regions;
        _cart = cart;
    }

    [HttpPost]
    public async Task<IActionResult> SubscribeEmail(string subscribeEmail, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(subscribeEmail) || !subscribeEmail.Contains('@'))
        {
            return Json("Geçersiz e-posta adresi.");
        }

        var exists = await _db.Subscribers.AsNoTracking()
            .AnyAsync(s => s.Email == subscribeEmail.Trim(), cancellationToken)
            .ConfigureAwait(false);

        if (!exists)
        {
            _db.Subscribers.Add(new Subscriber
            {
                Name = subscribeEmail.Trim(),
                Email = subscribeEmail.Trim(),
                Note = "Main-Page-Product-Subscription",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                Position = 1,
                Lang = SiteOptions.MainLanguage
            });
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return Json("success");
    }

    [HttpGet]
    public IActionResult GetAllCities()
    {
        var cities = _regions.GetAllCities();
        if (cities.Count == 0)
        {
            cities = ["Adana", "Ankara", "Antalya", "Bursa", "İstanbul", "İzmir"];
        }

        return Json(cities.Select(c => new { value = c, text = c }));
    }

    [HttpGet]
    public IActionResult GetTownsByCity(string city)
        => Json(_regions.GetTownsByCity(city ?? string.Empty).Select(t => new { value = t, text = t }));

    [HttpGet]
    public IActionResult GetDistrictsByTown(string city, string town)
        => Json(_regions.GetDistrictsByTown(city ?? string.Empty, town ?? string.Empty)
            .Select(d => new { value = d, text = d }));

    [HttpGet]
    public IActionResult GetIller() => GetAllCities();

    [HttpGet]
    public IActionResult GetIlceler(string il) => GetTownsByCity(il);

    [HttpGet]
    public async Task<IActionResult> HomePageShoppingCart(CancellationToken cancellationToken)
    {
        var cart = await _cart.GetCartAsync(cancellationToken).ConfigureAwait(false);
        return Json(new
        {
            count = cart.Lines.Sum(l => l.Quantity),
            total = cart.Total,
            lines = cart.Lines.Select(l => new { l.ProductId, l.Name, l.Quantity, l.UnitPrice, l.LineTotal })
        });
    }
}
