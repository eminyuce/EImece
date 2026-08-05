using EImece.Domain.Core.Caching;
using EImece.Domain.Core.Captcha;
using EImece.Domain.Core.Configuration;
using EImece.Domain.Core.Data;
using EImece.Domain.Core.Media;
using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

/// <summary>
/// Image resize / logo / captcha endpoints (SkiaSharp pipeline — Phase 8).
/// </summary>
public sealed class ImagesController : BaseController
{
    private const string CaptchaSessionPrefix = "Captcha";

    private readonly EImeceDbContext _db;
    private readonly IMediaFileService _media;
    private readonly IImageProcessingService _images;
    private readonly ICaptchaChallengeService _captcha;
    private readonly IEimeceCacheProvider _cache;
    private readonly CacheOptions _cacheOptions;

    public ImagesController(
        IOptions<EImeceOptions> siteOptions,
        EImeceDbContext db,
        IMediaFileService media,
        IImageProcessingService images,
        ICaptchaChallengeService captcha,
        IEimeceCacheProvider cache,
        IOptions<CacheOptions> cacheOptions)
        : base(siteOptions)
    {
        _db = db;
        _media = media;
        _images = images;
        _captcha = captcha;
        _cache = cache;
        _cacheOptions = cacheOptions.Value;
    }

    [HttpGet]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "imageSize", "id" })]
    public async Task<IActionResult> Index(string imageSize, string? id, CancellationToken cancellationToken)
    {
        var (width, height) = ImageSizeParser.Parse(imageSize);
        var fileStorageId = ImageSizeParser.ParseFileStorageId(id);
        if (fileStorageId <= 0)
        {
            return DefaultImageResult(width, height);
        }

        var cacheKey = $"img:{fileStorageId}:{width}x{height}";
        if (_cache.Get<byte[]>(cacheKey, out var cached) && cached is { Length: > 0 })
        {
            return File(cached, "image/jpeg");
        }

        try
        {
            var file = await _db.FileStorages.AsNoTracking()
                .Where(f => f.Id == fileStorageId)
                .Select(f => new { f.Id, f.FileName, f.MimeType, f.UpdatedDate })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (file?.FileName is null)
            {
                return DefaultImageResult(width, height);
            }

            var relative = Path.Combine("images", file.FileName).Replace('\\', '/');
            await using var stream = _media.OpenRead(relative)
                ?? _media.OpenRead(file.FileName);
            if (stream is null)
            {
                return DefaultImageResult(width, height);
            }

            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            var processed = _images.Resize(ms.ToArray(), width, height, file.MimeType);
            _cache.Set(cacheKey, processed.Bytes, _cacheOptions.LongSeconds);
            Response.Headers.LastModified = file.UpdatedDate.ToUniversalTime().ToString("R");
            return File(processed.Bytes, processed.ContentType);
        }
        catch
        {
            return DefaultImageResult(width, height);
        }
    }

    [HttpGet]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public IActionResult Logo()
    {
        const string cacheKey = "img:logo";
        if (_cache.Get<byte[]>(cacheKey, out var cached) && cached is { Length: > 0 })
        {
            return File(cached, "image/jpeg");
        }

        foreach (var candidate in new[] { "images/logo.jpg", "logo.jpg", "images/logo.png" })
        {
            using var stream = _media.OpenRead(candidate);
            if (stream is null)
            {
                continue;
            }

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var bytes = ms.ToArray();
            _cache.Set(cacheKey, bytes, _cacheOptions.VeryLongSeconds);
            var contentType = candidate.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";
            return File(bytes, contentType);
        }

        var placeholder = _images.CreatePlaceholder(200, 60, "EImece");
        _cache.Set(cacheKey, placeholder.Bytes, _cacheOptions.MediumSeconds);
        return File(placeholder.Bytes, placeholder.ContentType);
    }

    [HttpGet]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "imageSize" })]
    public IActionResult DefaultImage(string imageSize)
    {
        var (width, height) = ImageSizeParser.Parse(imageSize);
        return DefaultImageResult(width, height);
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult GetCaptcha(string? prefix, bool noisy = true)
    {
        var challenge = _captcha.CreateArithmeticChallenge(noisy);
        HttpContext.Session.SetInt32(CaptchaSessionPrefix + (prefix ?? string.Empty), challenge.Answer);
        return File(challenge.ImageBytes, challenge.ContentType);
    }

    private FileContentResult DefaultImageResult(int width, int height)
    {
        var cacheKey = $"img:default:{width}x{height}";
        if (_cache.Get<byte[]>(cacheKey, out var cached) && cached is { Length: > 0 })
        {
            return File(cached, "image/jpeg");
        }

        var image = _images.CreatePlaceholder(width, height);
        _cache.Set(cacheKey, image.Bytes, _cacheOptions.LongSeconds);
        return File(image.Bytes, image.ContentType);
    }
}
