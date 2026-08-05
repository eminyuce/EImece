namespace EImece.Domain.Core.Media;

public interface IImageProcessingService
{
    ProcessedImage Resize(byte[] sourceBytes, int width, int height, string? preferredContentType = null);
    ProcessedImage CreatePlaceholder(int width, int height, string text = "EImece");
    ProcessedImage CreateCaptchaImage(string text, bool includeNoise = true);
    Task SaveUploadAsync(Stream content, string fileName, int? maxWidth, int? maxHeight, CancellationToken cancellationToken = default);
}
