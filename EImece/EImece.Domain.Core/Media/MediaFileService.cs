using EImece.Domain.Core.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EImece.Domain.Core.Media;

public sealed class MediaFileService : IMediaFileService
{
    private readonly MediaOptions _options;
    private readonly ILogger<MediaFileService> _logger;

    public MediaFileService(
        IHostEnvironment environment,
        IOptions<MediaOptions> options,
        ILogger<MediaFileService> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Default RootRelativePath is "wwwroot/media" so ContentRoot resolves like legacy ~/media.
        MediaRootPath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, _options.RootRelativePath));
        ImagesPath = Path.Combine(MediaRootPath, _options.ImagesSubPath);
        TempPath = Path.Combine(MediaRootPath, _options.TempSubPath);
        UrlBase = _options.UrlBase.EndsWith('/') ? _options.UrlBase : _options.UrlBase + "/";
    }

    public string MediaRootPath { get; }
    public string ImagesPath { get; }
    public string TempPath { get; }
    public string UrlBase { get; }

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(MediaRootPath);
        Directory.CreateDirectory(ImagesPath);
        Directory.CreateDirectory(TempPath);
        _logger.LogDebug("Media directories ensured under {MediaRoot}", MediaRootPath);
    }

    public bool Exists(string relativePath)
    {
        var full = ResolveSafe(relativePath);
        return full is not null && File.Exists(full);
    }

    public Stream? OpenRead(string relativePath)
    {
        var full = ResolveSafe(relativePath);
        return full is null || !File.Exists(full)
            ? null
            : File.OpenRead(full);
    }

    public string? GetFullPath(string relativePath) => ResolveSafe(relativePath);

    public async Task WriteAsync(string relativePath, byte[] content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        EnsureDirectories();
        var full = ResolveSafe(relativePath)
            ?? throw new InvalidOperationException("Invalid media path.");
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await File.WriteAllBytesAsync(full, content, cancellationToken).ConfigureAwait(false);
    }

    public IEnumerable<string> ListFiles(string relativeDirectory, string searchPattern = "*")
    {
        var full = ResolveSafe(relativeDirectory);
        if (full is null || !Directory.Exists(full))
        {
            return Array.Empty<string>();
        }

        return Directory.EnumerateFiles(full, searchPattern);
    }

    private string? ResolveSafe(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return MediaRootPath;
        }

        var combined = Path.GetFullPath(Path.Combine(MediaRootPath, relativePath.TrimStart('/', '\\')));
        if (!combined.StartsWith(MediaRootPath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Blocked path traversal attempt: {RelativePath}", relativePath);
            return null;
        }

        return combined;
    }
}
