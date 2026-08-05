using EImece.Domain.Core.Media;
using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class FileUploadController : BaseAdminController
{
    private readonly IMediaFileService _media;

    public FileUploadController(IOptions<EImeceOptions> siteOptions, IMediaFileService media) : base(siteOptions)
    {
        _media = media;
    }

    [HttpGet]
    public IActionResult Index()
    {
        _media.EnsureDirectories();
        return View("~/Areas/Admin/Views/Shared/FileUpload.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            SetTempStatus("Dosya seçilmedi", isError: true);
            return RedirectToAction(nameof(Index));
        }

        _media.EnsureDirectories();
        var safeName = Path.GetFileName(file.FileName);
        var target = Path.Combine(_media.ImagesPath, $"{DateTime.UtcNow:yyyyMMddHHmmss}-{safeName}");
        await using (var stream = System.IO.File.Create(target))
        {
            await file.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
        }

        SetTempStatus("Yüklendi: " + Path.GetFileName(target));
        return RedirectToAction(nameof(Index));
    }
}
