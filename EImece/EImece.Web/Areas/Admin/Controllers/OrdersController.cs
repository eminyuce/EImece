using EImece.Domain.Core.Data;
using EImece.Web.Configuration;
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

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var count = await _db.Orders.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
            return AdminPlaceholder("Admin Orders", $"Orders admin shell · DB order count = {count}. Full order ops in later phases.");
        }
        catch
        {
            return AdminPlaceholder("Admin Orders", "Orders admin shell · database unavailable.");
        }
    }
}
