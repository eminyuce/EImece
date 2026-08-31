namespace EImece.Domain.Configuration
{
    /// <summary>
    /// Outbound HTTP API endpoints and credentials for non-payment integrations (reCAPTCHA, Bitly, etc.).
    /// </summary>
    public sealed class OutboundHttpOptions
    {
        public string RecaptchaSiteVerifyUrl { get; set; } = "https://www.google.com/recaptcha/api/siteverify";

        public string RecaptchaSecretKey { get; set; } = string.Empty;

        public string BitlyApiBaseUrl { get; set; } = "https://api-ssl.bitly.com";

        public static OutboundHttpOptions FromAppConfig()
        {
            return Cached.Value;
        }

        internal static void ResetForTests()
        {
            Cached = new System.Lazy<OutboundHttpOptions>(BuildFromAppConfig);
        }

        private static System.Lazy<OutboundHttpOptions> Cached = new System.Lazy<OutboundHttpOptions>(BuildFromAppConfig);

        private static OutboundHttpOptions BuildFromAppConfig()
        {
            return new OutboundHttpOptions
            {
                RecaptchaSiteVerifyUrl = AppConfig.RecaptchaSiteVerifyUrl,
                RecaptchaSecretKey = AppConfig.RecaptchaSecretKey ?? string.Empty,
                BitlyApiBaseUrl = AppConfig.GetConfigString("BitlyApiBaseUrl", "https://api-ssl.bitly.com"),
            };
        }
    }
}
