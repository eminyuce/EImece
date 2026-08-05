using EImece.Domain.Core.Data;
using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class ProductsController : BaseAdminController
{
    private readonly EImeceDbContext _db;

    public ProductsController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db)
        : base(siteOptions)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var count = await _db.Products.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
            return AdminPlaceholder("Admin Products", $"Product admin shell · DB product count = {count}. Full CRUD in later phases.");
        }
        catch
        {
            return AdminPlaceholder("Admin Products", "Product admin shell · database unavailable.");
        }
    }
}
