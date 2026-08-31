using EImece.Domain.Observability;
using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Logging;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;

namespace EImece.App_Start
{
    public static class ObservabilityBootstrap
    {
        private static OpenTelemetryBootstrap _openTelemetry;

        /// <summary>
        /// The OpenTelemetry SDK instance kept alive for the AppDomain lifetime.
        /// </summary>
        public static OpenTelemetryBootstrap OpenTelemetry => _openTelemetry;

        public static void Configure()
        {
            var options = ObservabilityOptions.FromAppConfig();
            var logger = LoggingBootstrap.LoggerFactory?.CreateLogger(typeof(ObservabilityBootstrap))
                ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            StructuredLoggingBootstrap.Configure(options);
            _openTelemetry = OpenTelemetryBootstrap.Initialize(options);
            ConfigureApplicationInsights(logger);

            logger.LogInformation(
                "Observability configured. Tracing={EnableTracing} Metrics={EnableMetrics} Otlp={HasOtlp} AzureMonitor={HasAzure} SamplingRatio={SamplingRatio}",
                options.EnableTracing,
                options.EnableMetrics,
                options.HasOtlpExporter,
                options.HasAzureMonitorExporter,
                options.SamplingRatio);
        }

        public static void Shutdown()
        {
            var logger = LoggingBootstrap.LoggerFactory?.CreateLogger(typeof(ObservabilityBootstrap));
            try
            {
                var telemetry = _openTelemetry;
                if (telemetry != null)
                {
                    telemetry.Dispose();
                    _openTelemetry = null;
                }

                OpenTelemetryBootstrap.Shutdown();
                StructuredLoggingBootstrap.CloseAndFlush();
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Observability shutdown encountered an error.");
            }
        }

        /// <summary>
        /// Applies the Application Insights connection string from environment or appSettings
        /// so deployments can avoid baking secrets into ApplicationInsights.config.
        /// </summary>
        private static void ConfigureApplicationInsights(ILogger logger)
        {
            try
            {
                var connectionString =
                    Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")
                    ?? Environment.GetEnvironmentVariable("APPINSIGHTS_CONNECTIONSTRING")
                    ?? ConfigurationManager.AppSettings["APPLICATIONINSIGHTS_CONNECTION_STRING"];

                var configuration = TelemetryConfiguration.CreateDefault();

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    if (string.IsNullOrWhiteSpace(configuration.ConnectionString))
                    {
                        configuration.DisableTelemetry = true;
                        logger.LogDebug("Application Insights disabled: no connection string in environment/appSettings or ApplicationInsights.config.");
                        return;
                    }

                    logger.LogDebug("Application Insights connection string not set in environment/appSettings; using ApplicationInsights.config.");
                    return;
                }

                configuration.ConnectionString = connectionString.Trim();
                logger.LogInformation("Application Insights connection string applied from environment/appSettings.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to configure Application Insights connection string.");
            }
        }
    }
}
