namespace EImece.Domain.Core.Media;

/// <summary>
/// Physical media root helper (legacy ~/media → ContentRoot/wwwroot/media).
/// </summary>
public interface IMediaFileService
{
    string MediaRootPath { get; }
    string ImagesPath { get; }
    string TempPath { get; }
    string UrlBase { get; }
    bool Exists(string relativePath);
    Stream? OpenRead(string relativePath);
    void EnsureDirectories();
    IEnumerable<string> ListFiles(string relativeDirectory, string searchPattern = "*");
}
