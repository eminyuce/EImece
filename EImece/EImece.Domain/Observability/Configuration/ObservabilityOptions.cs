using System;
using System.Configuration;
using System.Globalization;

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
        /// When true, builds TracerProvider and records server/client Activities.
        /// Exporters remain off until <see cref="OtlpEndpoint"/> or Azure Monitor is configured.
        /// </summary>
        public bool EnableTracing { get; set; }

        /// <summary>
        /// OTLP collector endpoint (e.g. http://localhost:4317). Prefer env var OTEL_EXPORTER_OTLP_ENDPOINT.
        /// </summary>
        public string OtlpEndpoint { get; set; }

        /// <summary>
        /// Trace sampling ratio in [0.0, 1.0]. Parent-based sampler uses this for root spans.
        /// </summary>
        public double SamplingRatio { get; set; } = 1.0;

        /// <summary>
        /// When true and a connection string is available from the environment, enables Azure Monitor exporters.
        /// </summary>
        public bool EnableAzureMonitorExporter { get; set; }

        /// <summary>
        /// Azure Monitor / Application Insights connection string. Loaded from environment only — never commit secrets.
        /// </summary>
        public string AzureMonitorConnectionString { get; set; }

        public string ServiceName { get; set; } = "EImece";

        public string ServiceVersion { get; set; }

        public string DeploymentEnvironment { get; set; }

        /// <summary>
        /// When true, Entity Framework generated SQL is written to application logs.
        /// Defaults to on for non-live environments; override with appSetting EnableEfSqlLogging.
        /// </summary>
        public bool EnableEfSqlLogging { get; set; }

        /// <summary>
        /// When true, records light DB Activities/metrics without logging full SQL text.
        /// </summary>
        public bool EnableEfTelemetry { get; set; } = true;

        public bool ExposeDetailedErrors { get; set; }

        public bool HasOtlpExporter
        {
            get { return !string.IsNullOrWhiteSpace(OtlpEndpoint); }
        }

        public bool HasAzureMonitorExporter
        {
            get
            {
                return EnableAzureMonitorExporter
                    && !string.IsNullOrWhiteSpace(AzureMonitorConnectionString);
            }
        }

        public static ObservabilityOptions FromAppConfig()
        {
            var defaultSampling = AppConfig.IsSiteLive ? 0.1d : 1.0d;
            var serviceVersion = typeof(ObservabilityOptions).Assembly.GetName().Version?.ToString() ?? "1.0.0";

            return new ObservabilityOptions
            {
                HttpTimeoutSeconds = AppConfig.GetConfigInt("HttpClientTimeoutSeconds", 30),
                HttpRetryCount = AppConfig.GetConfigInt("HttpClientRetryCount", 3),
                HttpCircuitBreakerFailures = AppConfig.GetConfigInt("HttpClientCircuitBreakerFailures", 5),
                HttpCircuitBreakerDurationSeconds = AppConfig.GetConfigInt("HttpClientCircuitBreakerDurationSeconds", 30),
                EnableRequestLogging = AppConfig.GetConfigBool("EnableRequestLogging", true),
                EnableMetrics = AppConfig.GetConfigBool("EnableMetrics", true),
                EnableTracing = AppConfig.GetConfigBool("EnableTracing", AppConfig.IsSiteUnderDevelopment),
                OtlpEndpoint = ResolveOtlpEndpoint(),
                SamplingRatio = ClampRatio(GetConfigDouble("SamplingRatio", defaultSampling)),
                EnableAzureMonitorExporter = AppConfig.GetConfigBool("EnableAzureMonitorExporter", false),
                AzureMonitorConnectionString = ResolveAzureMonitorConnectionString(),
                ServiceName = AppConfig.GetConfigString("OtelServiceName", "EImece"),
                ServiceVersion = AppConfig.GetConfigString("OtelServiceVersion", serviceVersion),
                DeploymentEnvironment = AppConfig.GetConfigString(
                    "OtelDeploymentEnvironment",
                    AppConfig.GetConfigString("SiteStatus", "dev")),
                EnableEfSqlLogging = AppConfig.GetConfigBool("EnableEfSqlLogging", AppConfig.IsSiteUnderDevelopment),
                EnableEfTelemetry = AppConfig.GetConfigBool("EnableEfTelemetry", true),
                ExposeDetailedErrors = AppConfig.IsSiteUnderDevelopment
            };
        }

        private static string ResolveOtlpEndpoint()
        {
            var fromEnv = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
            if (!string.IsNullOrWhiteSpace(fromEnv))
            {
                return fromEnv.Trim();
            }

            return AppConfig.GetConfigString("OtlpEndpoint", string.Empty);
        }

        /// <summary>
        /// Connection string is read from environment variables only (never from committed appSettings secrets).
        /// </summary>
        private static string ResolveAzureMonitorConnectionString()
        {
            return FirstNonEmpty(
                Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING"),
                Environment.GetEnvironmentVariable("APPINSIGHTS_CONNECTIONSTRING"),
                Environment.GetEnvironmentVariable("AZURE_MONITOR_CONNECTION_STRING"));
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null)
            {
                return null;
            }

            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
        }

        private static double GetConfigDouble(string configName, double defaultValue)
        {
            var raw = ConfigurationManager.AppSettings[configName];
            if (string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            double parsed;
            if (double.TryParse(raw.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
            {
                return parsed;
            }

            return defaultValue;
        }

        private static double ClampRatio(double value)
        {
            if (value < 0d)
            {
                return 0d;
            }

            if (value > 1d)
            {
                return 1d;
            }

            return value;
        }
    }
}
