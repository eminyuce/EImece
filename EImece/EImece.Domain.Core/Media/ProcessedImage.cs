namespace EImece.Domain.Core.Media;

public sealed class ProcessedImage
{
    public required byte[] Bytes { get; init; }
    public string ContentType { get; init; } = "image/jpeg";
    public DateTimeOffset? LastModified { get; init; }
}

public static class ImageSizeParser
{
    /// <summary>Parses legacy tokens like w150h150 into width/height (defaults 150×150).</summary>
    public static (int Width, int Height) Parse(string? imageSize)
    {
        if (string.IsNullOrWhiteSpace(imageSize))
        {
            return (150, 150);
        }

        var widthMatch = System.Text.RegularExpressions.Regex.Match(imageSize, @"w(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        var heightMatch = System.Text.RegularExpressions.Regex.Match(imageSize, @"h(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        var width = widthMatch.Success && int.TryParse(widthMatch.Groups[1].Value, out var w) ? w : 0;
        var height = heightMatch.Success && int.TryParse(heightMatch.Groups[1].Value, out var h) ? h : 0;

        if (width == 0 && height > 0)
        {
            width = height;
        }

        if (height == 0 && width > 0)
        {
            height = width;
        }

        if (width == 0 && height == 0)
        {
            return (150, 150);
        }

        width = Math.Clamp(width, 1, 4000);
        height = Math.Clamp(height, 1, 4000);
        return (width, height);
    }

    public static int ParseFileStorageId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return 0;
        }

        var cleaned = id.Replace(".jpg", "", StringComparison.OrdinalIgnoreCase)
            .Replace(".jpeg", "", StringComparison.OrdinalIgnoreCase)
            .Replace(".png", "", StringComparison.OrdinalIgnoreCase)
            .Replace(".webp", "", StringComparison.OrdinalIgnoreCase);

        // Legacy GetId() often takes trailing digits from slug-id patterns.
        var match = System.Text.RegularExpressions.Regex.Match(cleaned, @"(\d+)$");
        return match.Success && int.TryParse(match.Groups[1].Value, out var n) ? n : 0;
    }
}
