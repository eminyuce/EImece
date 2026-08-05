using EImece.Domain.Core.Data;
using EImece.Web.Configuration;
using EImece.Web.Helpers;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class OrdersController : BaseAdminController
{
    private readonly EImeceDbContext _db;

    public OrdersController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db)
        : base(siteOptions)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 25, string? sort = null, string? sortDir = null, CancellationToken cancellationToken = default)
    {
        var grid = GridQuery(search, page, pageSize, sort, sortDir);
        try
        {
            var query = _db.Orders.AsNoTracking()
                .Where(p => string.IsNullOrWhiteSpace(grid.Search)
                    || (p.OrderNumber != null && p.OrderNumber.Contains(grid.Search))
                    || (p.Name != null && p.Name.Contains(grid.Search))
                    || (p.PaymentStatus != null && p.PaymentStatus.Contains(grid.Search)));

            query = (grid.Sort?.ToLowerInvariant()) switch
            {
                "ordernumber" => grid.SortDir == "asc" ? query.OrderBy(p => p.OrderNumber) : query.OrderByDescending(p => p.OrderNumber),
                "name" => grid.SortDir == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "payment" => grid.SortDir == "asc" ? query.OrderBy(p => p.PaymentStatus) : query.OrderByDescending(p => p.PaymentStatus),
                "paid" => grid.SortDir == "asc" ? query.OrderBy(p => p.PaidPrice) : query.OrderByDescending(p => p.PaidPrice),
                "created" => grid.SortDir == "asc" ? query.OrderBy(p => p.CreatedDate) : query.OrderByDescending(p => p.CreatedDate),
                _ => query.OrderByDescending(p => p.Id)
            };

            var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await query.ApplyPaging(grid)
                .Select(p => new { p.Id, p.OrderNumber, p.Name, p.PaymentStatus, p.PaidPrice, p.CreatedDate })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            var rows = items.Select(x => (IReadOnlyList<string?>)new string?[]
            {
                x.Id.ToString(),
                x.OrderNumber,
                x.Name,
                x.PaymentStatus,
                x.PaidPrice,
                x.CreatedDate.ToString("yyyy-MM-dd HH:mm")
            });

            return EntityList(BuildList("Siparişler", "Orders",
                new[] { "Id", "OrderNumber", "Name", "Payment", "Paid", "Created" }, rows, grid.Search,
                showCreate: false, editAction: "Details", totalCount: total, grid: grid));
        }
        catch (Exception ex)
        {
            return EntityList(BuildList("Siparişler", "Orders",
                new[] { "Id", "OrderNumber", "Name", "Payment", "Paid", "Created" },
                Array.Empty<IReadOnlyList<string?>>(), grid.Search, ex.Message, showCreate: false, editAction: "Details", grid: grid));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.OrderProducts)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken).ConfigureAwait(false);
        if (order is null) return NotFound();

        var model = new OrderDetailsViewModel
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Name = order.Name,
            OrderStatus = order.OrderStatus,
            PaymentStatus = order.PaymentStatus,
            PaidPrice = order.PaidPrice,
            Price = order.Price,
            CargoPrice = order.CargoPrice,
            AdminOrderNote = order.AdminOrderNote,
            OrderComments = order.OrderComments,
            Coupon = order.Coupon,
            ShipmentCompanyName = order.ShipmentCompanyName,
            ShipmentTrackingNumber = order.ShipmentTrackingNumber,
            CreatedDate = order.CreatedDate,
            Lines = order.OrderProducts.Select(op => new OrderLineRow
            {
                ProductName = op.ProductName,
                ProductCode = op.ProductCode,
                Quantity = op.Quantity,
                Price = op.Price,
                TotalPrice = op.TotalPrice
            }).ToList()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(OrderUpdateViewModel model, CancellationToken cancellationToken)
    {
        var entity = await _db.Orders.FirstOrDefaultAsync(o => o.Id == model.Id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();

        entity.OrderStatus = model.OrderStatus;
        entity.AdminOrderNote = model.AdminOrderNote;
        entity.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Sipariş güncellendi");
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }
}
