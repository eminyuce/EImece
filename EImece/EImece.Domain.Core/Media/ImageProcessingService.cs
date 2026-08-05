using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace EImece.Domain.Core.Media;

public sealed class ImageProcessingService : IImageProcessingService
{
    private readonly IMediaFileService _media;
    private readonly ILogger<ImageProcessingService> _logger;

    public ImageProcessingService(IMediaFileService media, ILogger<ImageProcessingService> logger)
    {
        _media = media;
        _logger = logger;
    }

    public ProcessedImage Resize(byte[] sourceBytes, int width, int height, string? preferredContentType = null)
    {
        ArgumentNullException.ThrowIfNull(sourceBytes);
        width = Math.Clamp(width, 1, 4000);
        height = Math.Clamp(height, 1, 4000);

        using var input = SKBitmap.Decode(sourceBytes)
            ?? throw new InvalidOperationException("Unable to decode image bytes.");

        using var resized = input.Resize(new SKImageInfo(width, height), SKFilterQuality.High)
            ?? throw new InvalidOperationException("Unable to resize image.");

        using var image = SKImage.FromBitmap(resized);
        var encodeFormat = preferredContentType?.Contains("png", StringComparison.OrdinalIgnoreCase) == true
            ? SKEncodedImageFormat.Png
            : SKEncodedImageFormat.Jpeg;
        using var data = image.Encode(encodeFormat, 85);

        return new ProcessedImage
        {
            Bytes = data.ToArray(),
            ContentType = encodeFormat == SKEncodedImageFormat.Png ? "image/png" : "image/jpeg"
        };
    }

    public ProcessedImage CreatePlaceholder(int width, int height, string text = "EImece")
    {
        width = Math.Clamp(width <= 0 ? 150 : width, 1, 4000);
        height = Math.Clamp(height <= 0 ? 150 : height, 1, 4000);

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.Clear(new SKColor(241, 245, 249));

        using var border = new SKPaint
        {
            Color = new SKColor(203, 213, 225),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        };
        canvas.DrawRect(1, 1, width - 2, height - 2, border);

        using var paint = new SKPaint
        {
            Color = new SKColor(71, 85, 105),
            IsAntialias = true,
            TextSize = Math.Max(12, Math.Min(width, height) / 6f),
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.Default
        };
        canvas.DrawText(text, width / 2f, height / 2f + paint.TextSize / 3f, paint);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);
        return new ProcessedImage { Bytes = data.ToArray(), ContentType = "image/jpeg" };
    }

    public ProcessedImage CreateCaptchaImage(string text, bool includeNoise = true)
    {
        const int width = 130;
        const int height = 30;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        if (includeNoise)
        {
            var rand = Random.Shared;
            using var pen = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, IsAntialias = true };
            for (var i = 0; i < 10; i++)
            {
                pen.Color = new SKColor((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256));
                var r = rand.Next(0, width / 3);
                var x = rand.Next(0, width);
                var y = rand.Next(0, height);
                canvas.DrawCircle(x, y, r, pen);
            }
        }

        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            TextSize = 16,
            Typeface = SKTypeface.FromFamilyName("sans-serif") ?? SKTypeface.Default
        };
        canvas.DrawText(text, 4, 22, textPaint);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        return new ProcessedImage { Bytes = data.ToArray(), ContentType = "image/jpeg" };
    }

    public async Task SaveUploadAsync(
        Stream content,
        string fileName,
        int? maxWidth,
        int? maxHeight,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("fileName is required.", nameof(fileName));
        }

        _media.EnsureDirectories();
        await using var ms = new MemoryStream();
        await content.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        var bytes = ms.ToArray();

        if (maxWidth is > 0 || maxHeight is > 0)
        {
            using var decoded = SKBitmap.Decode(bytes);
            if (decoded is not null)
            {
                var targetW = maxWidth is > 0 ? Math.Min(decoded.Width, maxWidth.Value) : decoded.Width;
                var targetH = maxHeight is > 0 ? Math.Min(decoded.Height, maxHeight.Value) : decoded.Height;
                if (targetW != decoded.Width || targetH != decoded.Height)
                {
                    bytes = Resize(bytes, targetW, targetH).Bytes;
                }
            }
        }

        var safeName = Path.GetFileName(fileName);
        var relative = Path.Combine("images", safeName).Replace('\\', '/');
        await _media.WriteAsync(relative, bytes, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Saved media upload {Relative} ({Bytes} bytes)", relative, bytes.Length);
    }
}
