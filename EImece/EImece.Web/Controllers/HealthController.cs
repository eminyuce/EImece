using EImece.Domain.Core.Data;
using EImece.Domain.Core.Repositories;
using EImece.Domain.Core.Entities;
using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

/// <summary>
/// Parity with legacy GET /health and GET /healthz endpoints.
/// Phase 3 adds optional EF Core connectivity probe (does not fail the host if DB is offline).
/// </summary>
[ApiController]
[Route("[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly EImeceOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly EImeceDbContext _db;
    private readonly IReadRepository<Product> _products;

    public HealthController(
        IOptions<EImeceOptions> options,
        IHostEnvironment environment,
        EImeceDbContext db,
        IReadRepository<Product> products)
    {
        _options = options.Value;
        _environment = environment;
        _db = db;
        _products = products;
    }

    [HttpGet("/health")]
    [HttpGet("/healthz")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        string databaseStatus;
        int? productCount = null;
        try
        {
            var canConnect = await _db.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
            if (canConnect)
            {
                productCount = await _products.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                databaseStatus = "UP";
            }
            else
            {
                databaseStatus = "DOWN";
            }
        }
        catch (Exception ex)
        {
            databaseStatus = $"UNAVAILABLE: {ex.GetType().Name}";
        }

        // Host stays UP even if DB is down during migration development.
        return Ok(new
        {
            Status = "UP",
            Host = "EImece.Web",
            Framework = "ASP.NET Core 8",
            Orm = "Entity Framework Core 8",
            Environment = _environment.EnvironmentName,
            SiteStatus = _options.SiteStatus,
            Database = databaseStatus,
            ProductCount = productCount,
            TimestampUtc = DateTime.UtcNow
        });
    }
}
