using EImece.Web.Configuration;
using EImece.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EImece.Web.Areas.Admin.Controllers;

public sealed class AppLogsController : BaseAdminController
{
    private readonly IWebHostEnvironment _env;

    public AppLogsController(IOptions<EImeceOptions> siteOptions, IWebHostEnvironment env) : base(siteOptions)
    {
        _env = env;
    }

    private string LogDirectory
    {
        get
        {
            var path = Path.Combine(_env.ContentRootPath, "App_Data", "logs");
            Directory.CreateDirectory(path);
            return path;
        }
    }

    [HttpGet]
    public IActionResult Index(string? file)
    {
        var logFiles = Directory.Exists(LogDirectory)
            ? Directory.GetFiles(LogDirectory, "*.*")
                .Select(f => new LogFileRow
                {
                    FileName = Path.GetFileName(f)!,
                    SizeBytes = new FileInfo(f).Length,
                    LastWriteUtc = System.IO.File.GetLastWriteTimeUtc(f)
                })
                .OrderByDescending(f => f.LastWriteUtc)
                .Take(50)
                .ToList()
            : new List<LogFileRow>();

        string? content = null;
        string? selected = null;
        if (!string.IsNullOrWhiteSpace(file))
        {
            selected = Path.GetFileName(file);
            var full = Path.Combine(LogDirectory, selected);
            if (System.IO.File.Exists(full))
            {
                var lines = System.IO.File.ReadLines(full).TakeLast(200);
                content = string.Join(Environment.NewLine, lines);
            }
        }

        return View(new AppLogsViewModel
        {
            LogDirectory = LogDirectory,
            LogFiles = logFiles,
            SelectedLogName = selected,
            SelectedLogContent = content
        });
    }
}
