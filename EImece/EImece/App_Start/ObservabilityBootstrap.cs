using EImece.Domain.Observability.Logging;
using Microsoft.ApplicationInsights.Extensibility;
using NLog;
using System;
using System.Configuration;

namespace EImece.App_Start
{
    public static class ObservabilityBootstrap
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Configure()
        {
            // The resilient HTTP client is now consumed via constructor injection
            // (see IImageDownloadService); no global static accessor to prime here.
            StructuredLoggingBootstrap.Configure();
            ConfigureApplicationInsights();
        }

        /// <summary>
        /// Applies the Application Insights connection string from environment or appSettings
        /// so deployments can avoid baking secrets into ApplicationInsights.config.
        /// Automatic request/dependency/exception/perf-counter collection is enabled via
        /// Microsoft.ApplicationInsights.Web 3.x + ApplicationInsights.config toggles.
        /// </summary>
        private static void ConfigureApplicationInsights()
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
                    // Avoid request-time crashes from ApplicationInsightsHttpModule when no CS is configured.
                    if (string.IsNullOrWhiteSpace(configuration.ConnectionString))
                    {
                        configuration.DisableTelemetry = true;
                        Logger.Debug("Application Insights disabled: no connection string in environment/appSettings or ApplicationInsights.config.");
                        return;
                    }

                    Logger.Debug("Application Insights connection string not set in environment/appSettings; using ApplicationInsights.config.");
                    return;
                }

                configuration.ConnectionString = connectionString.Trim();
                Logger.Info("Application Insights connection string applied from environment/appSettings.");
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to configure Application Insights connection string.");
            }
        }
    }
}
