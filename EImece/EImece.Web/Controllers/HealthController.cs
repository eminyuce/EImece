using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

/// <summary>
/// Parity with legacy GET /health and GET /healthz endpoints.
/// Phase 2 returns host readiness only (no DB checks yet).
/// </summary>
[ApiController]
[Route("[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly EImeceOptions _options;
    private readonly IHostEnvironment _environment;

    public HealthController(IOptions<EImeceOptions> options, IHostEnvironment environment)
    {
        _options = options.Value;
        _environment = environment;
    }

    [HttpGet("/health")]
    [HttpGet("/healthz")]
    public IActionResult Get()
    {
        return Ok(new
        {
            Status = "UP",
            Host = "EImece.Web",
            Framework = "ASP.NET Core 8",
            Environment = _environment.EnvironmentName,
            SiteStatus = _options.SiteStatus,
            TimestampUtc = DateTime.UtcNow
        });
    }
}
