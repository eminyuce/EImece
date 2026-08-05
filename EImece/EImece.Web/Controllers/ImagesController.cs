using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Controllers;

/// <summary>
/// Image route shells — System.Drawing/ImageProcessor pipeline migrates in Phase 8 (SkiaSharp).
/// </summary>
public sealed class ImagesController : BaseController
{
    public ImagesController(IOptions<EImeceOptions> siteOptions) : base(siteOptions) { }

    [HttpGet]
    public IActionResult Index(string imageSize, string? id)
        => StatusCode(StatusCodes.Status501NotImplemented,
            $"Image resize /images/{imageSize}/{id} not migrated yet (Phase 8).");

    [HttpGet]
    public IActionResult Logo()
        => StatusCode(StatusCodes.Status501NotImplemented, "Logo image endpoint migrates in Phase 8.");

    [HttpGet]
    public IActionResult DefaultImage(string imageSize)
        => StatusCode(StatusCodes.Status501NotImplemented, $"Default image /images/defaultImage/{imageSize}/default.jpg — Phase 8.");

    [HttpGet]
    public IActionResult GetCaptcha()
        => StatusCode(StatusCodes.Status501NotImplemented, "Captcha image migrates in Phase 8.");
}
