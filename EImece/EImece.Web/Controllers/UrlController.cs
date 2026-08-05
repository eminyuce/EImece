using System.ComponentModel.DataAnnotations;
using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Domain.Core.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EImece.Web.Controllers;

/// <summary>
/// Short-URL API (legacy Web API UrlController → ASP.NET Core).
/// </summary>
[ApiController]
[Route("api/url")]
[Authorize(Policy = AuthPolicies.AdminOnly)]
public sealed class UrlController : ControllerBase
{
    private readonly EImeceDbContext _db;

    public UrlController(EImeceDbContext db)
    {
        _db = db;
    }

    public sealed class ShortUrlRequest
    {
        [Required]
        [Url]
        public string Url { get; set; } = string.Empty;
    }

    [HttpGet("{key}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(string key, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return BadRequest(new ProblemDetails { Title = "Key is required." });
        }

        try
        {
            var entity = await _db.ShortUrls.AsNoTracking()
                .FirstOrDefaultAsync(s => s.UrlKey == key, cancellationToken)
                .ConfigureAwait(false);

            if (entity is null || string.IsNullOrWhiteSpace(entity.Url))
            {
                return NotFound(new ProblemDetails { Title = "Short URL not found." });
            }

            if (!Uri.TryCreate(entity.Url, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return BadRequest(new ProblemDetails { Title = "Stored URL is not a safe absolute http(s) URL." });
            }

            return RedirectPermanent(uri.ToString());
        }
        catch
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails { Title = "Database unavailable." });
        }
    }

    [HttpPost("short")]
    public async Task<IActionResult> Create([FromBody] ShortUrlRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return BadRequest(new ProblemDetails { Title = "URL must be absolute http(s)." });
        }

        try
        {
            var key = Guid.NewGuid().ToString("N")[..8];
            var entity = new ShortUrl
            {
                Name = key,
                UrlKey = key,
                Url = uri.ToString(),
                RequestCount = 0,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                Position = 0,
                Lang = 1
            };
            _db.ShortUrls.Add(entity);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Ok(new { key, url = uri.ToString() });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails { Title = "Could not create short URL.", Detail = ex.GetType().Name });
        }
    }
}
