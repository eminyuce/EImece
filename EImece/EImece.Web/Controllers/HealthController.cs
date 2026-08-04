using EImece.Domain.Core.Caching;
using EImece.Domain.Core.Configuration;
using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Domain.Core.Media;
using EImece.Domain.Core.Repositories;
using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

/// <summary>
/// Parity with legacy GET /health and GET /healthz endpoints.
/// Phase 4 adds media/cache/scheduler probes (host stays UP if DB is offline).
/// </summary>
[ApiController]
[Route("[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly EImeceOptions _options;
    private readonly QuartzOptions _quartz;
    private readonly CacheOptions _cacheOptions;
    private readonly IHostEnvironment _environment;
    private readonly EImeceDbContext _db;
    private readonly IReadRepository<Product> _products;
    private readonly IMediaFileService _media;
    private readonly IEimeceCacheProvider _cache;

    public HealthController(
        IOptions<EImeceOptions> options,
        IOptions<QuartzOptions> quartz,
        IOptions<CacheOptions> cacheOptions,
        IHostEnvironment environment,
        EImeceDbContext db,
        IReadRepository<Product> products,
        IMediaFileService media,
        IEimeceCacheProvider cache)
    {
        _options = options.Value;
        _quartz = quartz.Value;
        _cacheOptions = cacheOptions.Value;
        _environment = environment;
        _db = db;
        _products = products;
        _media = media;
        _cache = cache;
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

        var cacheProbeKey = "health:probe";
        _cache.Set(cacheProbeKey, DateTime.UtcNow.Ticks, _cacheOptions.ShortSeconds);
        var cacheOk = _cache.Get<long>(cacheProbeKey, out _);

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
            Media = new
            {
                Root = _media.MediaRootPath,
                ImagesExists = Directory.Exists(_media.ImagesPath),
                TempExists = Directory.Exists(_media.TempPath)
            },
            Cache = new
            {
                Active = _cacheOptions.IsCacheActive,
                ProbeOk = cacheOk
            },
            Scheduler = new
            {
                Enabled = _quartz.IsEnabled
            },
            TimestampUtc = DateTime.UtcNow
        });
    }
}
