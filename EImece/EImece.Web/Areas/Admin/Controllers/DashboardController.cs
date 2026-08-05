using EImece.Domain.Core.Data;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class DashboardController : BaseAdminController
{
    private readonly EImeceDbContext _db;

    public DashboardController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db) : base(siteOptions)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var model = new DashboardViewModel
            {
                ProductCount = await _db.Products.CountAsync(cancellationToken).ConfigureAwait(false),
                OrderCount = await _db.Orders.CountAsync(cancellationToken).ConfigureAwait(false),
                CustomerCount = await _db.Customers.CountAsync(cancellationToken).ConfigureAwait(false),
                SubscriberCount = await _db.Subscribers.CountAsync(cancellationToken).ConfigureAwait(false),
                RecentOrders = await _db.Orders.AsNoTracking()
                    .OrderByDescending(o => o.Id)
                    .Take(10)
                    .Select(o => new RecentOrderRow
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        Name = o.Name,
                        PaymentStatus = o.PaymentStatus,
                        PaidPrice = o.PaidPrice,
                        CreatedDate = o.CreatedDate
                    })
                    .ToListAsync(cancellationToken).ConfigureAwait(false)
            };
            return View(model);
        }
        catch (Exception ex)
        {
            SetTempStatus("Dashboard yüklenemedi: " + ex.Message, isError: true);
            return View(new DashboardViewModel());
        }
    }

    [HttpGet]
    public IActionResult ClearCache()
    {
        SetTempStatus("Önbellek temizleme isteği alındı (Core).");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult OurSiteFeatures()
    {
        ViewData["Title"] = "Sitemizin Özellikleri";
        ViewData["Message"] = "Site özellikleri sayfası (Core).";
        return View("~/Areas/Admin/Views/Shared/Placeholder.cshtml");
    }
}
