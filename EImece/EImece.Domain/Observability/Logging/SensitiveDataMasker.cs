using System;
using System.Text.RegularExpressions;

namespace EImece.Domain.Observability.Logging
{
    public static class SensitiveDataMasker
    {
        // Use \S+ for values (Mono mishandles \s inside some character classes).
        private static readonly Regex PasswordPattern = new Regex(
            @"\b(password|pwd|secret|token|apikey|api_key|connectionstring|connection_string)\s*[:=]\s*\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        private static readonly Regex AuthorizationBearerPattern = new Regex(
            @"\bauthorization\s*[:=]\s*Bearer\s+\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        private static readonly Regex BearerPattern = new Regex(
            @"Bearer\s+\S+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        private static readonly Regex CardNumberPattern = new Regex(
            @"\b(?:\d[ -]*?){13,19}\b",
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        private static readonly Regex Cv2Pattern = new Regex(
            @"\b(cvc|cvv|cv2)\s*[:=]\s*\d{3,4}\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        private static readonly Regex ConnectionStringPattern = new Regex(
            @"(Password|Pwd|User ID|UserId|AccountKey|SharedAccessKey)\s*=\s*[^;""'\s]+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        public static string Mask(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            // Authorization/Bearer first so generic key=value masking does not consume tokens.
            var masked = AuthorizationBearerPattern.Replace(value, "authorization=Bearer ***");
            masked = BearerPattern.Replace(masked, "Bearer ***");
            masked = PasswordPattern.Replace(masked, MatchKeyValue);
            masked = ConnectionStringPattern.Replace(masked, "$1=***");
            masked = Cv2Pattern.Replace(masked, "$1=***");
            masked = CardNumberPattern.Replace(masked, "****-****-****-****");
            return masked;
        }

        private static string MatchKeyValue(Match match)
        {
            return match.Groups[1].Value + "=***";
        }
    }
}
