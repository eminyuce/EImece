using EImece.Domain.Core.Helpers;
using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class ImportDataController : BaseAdminController
{
    private readonly IWebHostEnvironment _env;

    public ImportDataController(IOptions<EImeceOptions> siteOptions, IWebHostEnvironment env) : base(siteOptions)
    {
        _env = env;
    }

    private string AppDataPath
    {
        get
        {
            var path = Path.Combine(_env.ContentRootPath, "App_Data");
            Directory.CreateDirectory(path);
            return path;
        }
    }

    [HttpGet]
    public IActionResult Index()
    {
        var files = Directory.Exists(AppDataPath)
            ? Directory.GetFiles(AppDataPath)
                .Select(Path.GetFileName)
                .Where(f => f is not null)
                .Cast<string>()
                .OrderBy(f => f)
                .ToArray()
            : Array.Empty<string>();

        return View(new ImportDataIndexViewModel
        {
            AppDataPath = AppDataPath,
            AppDataFiles = files
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcelUpload(IFormFile? excelFile, CancellationToken cancellationToken)
    {
        if (excelFile is null || excelFile.Length == 0)
        {
            SetTempStatus("Excel dosyası seçilmedi", isError: true);
            return RedirectToAction(nameof(Index));
        }

        var safeName = Path.GetFileName(excelFile.FileName);
        var stored = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{safeName}";
        var fullPath = Path.Combine(AppDataPath, stored);
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await excelFile.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
        }

        return RedirectToAction(nameof(DisplayTable), new { id = stored });
    }

    [HttpGet]
    public IActionResult DisplayTable(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction(nameof(Index));
        }

        var fullPath = Path.Combine(AppDataPath, Path.GetFileName(id));
        if (!System.IO.File.Exists(fullPath))
        {
            SetTempStatus("Dosya bulunamadı", isError: true);
            return RedirectToAction(nameof(Index));
        }

        var table = ExcelPreviewHelper.ReadFirstSheet(fullPath);
        return View(new ImportPreviewViewModel
        {
            FileName = Path.GetFileName(id),
            Preview = table
        });
    }
}
