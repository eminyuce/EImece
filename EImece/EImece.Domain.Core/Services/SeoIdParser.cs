using System.Text.RegularExpressions;

namespace EImece.Domain.Core.Services;

public static partial class SeoIdParser
{
    public static int Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        if (int.TryParse(value, out var direct))
        {
            return direct;
        }

        var match = TrailingDigits().Match(value.Trim());
        return match.Success && int.TryParse(match.Groups[1].Value, out var id) ? id : 0;
    }

    [GeneratedRegex(@"(\d+)$", RegexOptions.CultureInvariant)]
    private static partial Regex TrailingDigits();
}
