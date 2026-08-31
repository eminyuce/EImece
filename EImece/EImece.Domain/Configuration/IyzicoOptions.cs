using System;

namespace EImece.Domain.Configuration
{
    /// <summary>
    /// Iyzico payment gateway settings bound from Web.config appSettings and environment variables.
    /// </summary>
    public sealed class IyzicoOptions
    {
        public string ApiKey { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;

        public string BaseUrl { get; set; } = "https://sandbox-api.iyzipay.com";

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(SecretKey);

        public static IyzicoOptions FromAppConfig()
        {
            return Cached.Value;
        }

        internal static void ResetForTests()
        {
            Cached = new Lazy<IyzicoOptions>(BuildFromAppConfig);
        }

        private static Lazy<IyzicoOptions> Cached = new Lazy<IyzicoOptions>(BuildFromAppConfig);

        private static IyzicoOptions BuildFromAppConfig()
        {
            return new IyzicoOptions
            {
                ApiKey = AppConfig.IyzicoApiKey ?? string.Empty,
                SecretKey = AppConfig.IyzicoSecretKey ?? string.Empty,
                BaseUrl = AppConfig.IyzicoBaseUrl ?? "https://sandbox-api.iyzipay.com",
            };
        }
    }
}
