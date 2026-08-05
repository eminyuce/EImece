using EImece.Domain.Core.Data;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class MetricsController : BaseAdminController
{
    private readonly EImeceDbContext _db;

    public MetricsController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db) : base(siteOptions)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var model = new MetricsViewModel
            {
                ProductCount = await _db.Products.CountAsync(cancellationToken).ConfigureAwait(false),
                OrderCount = await _db.Orders.CountAsync(cancellationToken).ConfigureAwait(false),
                CustomerCount = await _db.Customers.CountAsync(cancellationToken).ConfigureAwait(false),
                SubscriberCount = await _db.Subscribers.CountAsync(cancellationToken).ConfigureAwait(false)
            };
            return View(model);
        }
        catch (Exception ex)
        {
            SetTempStatus("Metrikler yüklenemedi: " + ex.Message, isError: true);
            return View(new MetricsViewModel());
        }
    }
}
