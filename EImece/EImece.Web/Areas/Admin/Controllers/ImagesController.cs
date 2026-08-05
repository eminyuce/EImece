using EImece.Domain.Core.Data;
using EImece.Domain.Core.Media;
using EImece.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class ImagesController : BaseAdminController
{
    private readonly EImeceDbContext _db;
    private readonly IMediaFileService _media;
    private readonly IImageProcessingService _images;

    public ImagesController(
        IOptions<EImeceOptions> siteOptions,
        EImeceDbContext db,
        IMediaFileService media,
        IImageProcessingService images)
        : base(siteOptions)
    {
        _db = db;
        _media = media;
        _images = images;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        _media.EnsureDirectories();
        var count = await _db.FileStorages.AsNoTracking().CountAsync(cancellationToken).ConfigureAwait(false);
        var diskFiles = _media.ListFiles("images").Take(50).Select(Path.GetFileName).ToList();
        ViewData["Title"] = "Görseller";
        ViewData["DbCount"] = count;
        ViewData["DiskFiles"] = diskFiles;
        ViewData["ImagesPath"] = _media.ImagesPath;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompressImages(CancellationToken cancellationToken)
    {
        _media.EnsureDirectories();
        var files = _media.ListFiles("images", "*.*").Take(100).ToList();
        var compressed = 0;
        foreach (var filePath in files)
        {
            try
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(filePath, cancellationToken).ConfigureAwait(false);
                if (bytes.Length < 50_000)
                {
                    continue;
                }

                var processed = _images.Resize(bytes, 1600, 1600);
                if (processed.Bytes.Length > 0 && processed.Bytes.Length < bytes.Length)
                {
                    await System.IO.File.WriteAllBytesAsync(filePath, processed.Bytes, cancellationToken).ConfigureAwait(false);
                    compressed++;
                }
            }
            catch
            {
                // skip unreadable files
            }
        }

        SetTempStatus($"{compressed} görsel sıkıştırıldı.");
        return RedirectToAction(nameof(Index));
    }
}
