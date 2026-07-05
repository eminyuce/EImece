using System.Text.RegularExpressions;

namespace EImece.Domain.Observability.Logging
{
    public static class SensitiveDataMasker
    {
        private static readonly Regex PasswordPattern = new Regex(@"(password|pwd|secret|token|apikey|api_key|authorization)\s*[:=]\s*[^,\s""']+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex BearerPattern = new Regex(@"Bearer\s+[A-Za-z0-9\-._~+/]+=*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string Mask(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var masked = PasswordPattern.Replace(value, "$1=***");
            masked = BearerPattern.Replace(masked, "Bearer ***");
            return masked;
        }
    }
}
