using EImece.Domain.Core.Data;
using EImece.Domain.Core.Entities;
using EImece.Domain.Core.Media;
using EImece.Web.Configuration;
using EImece.Web.Helpers;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class MediaController : BaseAdminController
{
    private readonly EImeceDbContext _db;
    private readonly IMediaFileService _media;

    public MediaController(IOptions<EImeceOptions> siteOptions, EImeceDbContext db, IMediaFileService media)
        : base(siteOptions)
    {
        _db = db;
        _media = media;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        int page = 1,
        int pageSize = 25,
        string? sort = null,
        string? sortDir = null,
        int contentId = 0,
        string? mod = null,
        string? imageType = null,
        CancellationToken cancellationToken = default)
    {
        _media.EnsureDirectories();
        ViewBag.ContentId = contentId;
        ViewBag.Mod = mod;
        ViewBag.ImageType = imageType;

        var grid = GridQuery(search, page, pageSize, sort, sortDir);
        var query = _db.FileStorages.AsNoTracking()
            .Where(f => string.IsNullOrWhiteSpace(grid.Search)
                || (f.FileName != null && f.FileName.Contains(grid.Search))
                || (f.FileUrl != null && f.FileUrl.Contains(grid.Search)));

        query = (grid.Sort?.ToLowerInvariant()) switch
        {
            "filename" or "dosya" => grid.SortDir == "asc" ? query.OrderBy(f => f.FileName) : query.OrderByDescending(f => f.FileName),
            "active" => grid.SortDir == "asc" ? query.OrderBy(f => f.IsActive) : query.OrderByDescending(f => f.IsActive),
            _ => query.OrderByDescending(f => f.Id)
        };

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query.ApplyPaging(grid)
            .Select(f => new { f.Id, f.FileName, f.FileUrl, f.MimeType, f.FileSize, f.IsActive })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var rows = items.Select(f => (IReadOnlyList<string?>)new string?[]
        {
            f.Id.ToString(),
            f.FileName,
            f.FileUrl,
            f.MimeType,
            f.FileSize.ToString(),
            f.IsActive ? "Evet" : "Hayır"
        });

        return EntityList(BuildList(
            "Medya",
            "Media",
            new[] { "Id", "FileName", "Url", "Mime", "Size", "Active" },
            rows,
            grid.Search,
            notice: $"Yükleme formu üstte · contentId={contentId}",
            showCreate: false,
            totalCount: total,
            grid: grid,
            ajaxDeleteAction: "DeleteMediaGridItem"));
    }

    [HttpGet]
    public IActionResult UploadForm(int contentId = 0, string? mod = null, string? imageType = null)
        => View("Upload", new MediaListViewModel { ContentId = contentId, Mod = mod, ImageType = imageType });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile? file, int contentId = 0, string? mod = null, string? imageType = null, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            SetTempStatus("Dosya seçilmedi", isError: true);
            return RedirectToAction(nameof(Index), new { contentId, mod, imageType });
        }

        _media.EnsureDirectories();
        var safeName = Path.GetFileName(file.FileName);
        var relative = Path.Combine("images", $"{DateTime.UtcNow:yyyyMMddHHmmss}_{safeName}").Replace('\\', '/');
        await using (var stream = file.OpenReadStream())
        using (var ms = new MemoryStream())
        {
            await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            await _media.WriteAsync(relative, ms.ToArray(), cancellationToken).ConfigureAwait(false);
        }

        _db.FileStorages.Add(new FileStorage
        {
            Name = safeName,
            FileName = safeName,
            FileUrl = _media.UrlBase.TrimEnd('/') + "/" + Path.GetFileName(relative),
            MimeType = file.ContentType,
            FileSize = (int)Math.Min(int.MaxValue, file.Length),
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            Lang = SiteOptions.MainLanguage,
            Position = 1
        });
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Dosya yüklendi");
        return RedirectToAction(nameof(Index), new { contentId, mod, imageType });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.FileStorages.FirstOrDefaultAsync(f => f.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return NotFound();
        entity.IsActive = false;
        entity.UpdatedDate = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        SetTempStatus("Medya pasifleştirildi");
        return RedirectToAction(nameof(Index));
    }
}
