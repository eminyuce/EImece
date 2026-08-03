using System;

namespace EImece.Domain.Observability.Configuration
{
    public sealed class ObservabilityOptions
    {
        public int HttpTimeoutSeconds { get; set; } = 30;

        public int HttpRetryCount { get; set; } = 3;

        public int HttpCircuitBreakerFailures { get; set; } = 5;

        public int HttpCircuitBreakerDurationSeconds { get; set; } = 30;

        public bool EnableRequestLogging { get; set; } = true;

        public bool EnableMetrics { get; set; } = true;

        /// <summary>
        /// When true, Entity Framework generated SQL is written to application logs.
        /// Defaults to on for non-live environments; override with appSetting EnableEfSqlLogging.
        /// </summary>
        public bool EnableEfSqlLogging { get; set; }

        public bool ExposeDetailedErrors { get; set; }

        public static ObservabilityOptions FromAppConfig()
        {
            return new ObservabilityOptions
            {
                HttpTimeoutSeconds = AppConfig.GetConfigInt("HttpClientTimeoutSeconds", 30),
                HttpRetryCount = AppConfig.GetConfigInt("HttpClientRetryCount", 3),
                HttpCircuitBreakerFailures = AppConfig.GetConfigInt("HttpClientCircuitBreakerFailures", 5),
                HttpCircuitBreakerDurationSeconds = AppConfig.GetConfigInt("HttpClientCircuitBreakerDurationSeconds", 30),
                EnableRequestLogging = AppConfig.GetConfigBool("EnableRequestLogging", true),
                EnableMetrics = AppConfig.GetConfigBool("EnableMetrics", true),
                EnableEfSqlLogging = AppConfig.GetConfigBool("EnableEfSqlLogging", AppConfig.IsSiteUnderDevelopment),
                ExposeDetailedErrors = AppConfig.IsSiteUnderDevelopment
            };
        }
    }
}
