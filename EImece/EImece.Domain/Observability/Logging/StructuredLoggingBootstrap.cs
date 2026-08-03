using EImece.Domain.Observability.Configuration;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System;
using System.IO;
using System.Web;

namespace EImece.Domain.Observability.Logging
{
    public static class StructuredLoggingBootstrap
    {
        private static bool _initialized;

        public static void Configure()
        {
            Configure(ObservabilityOptions.FromAppConfig());
        }

        public static void Configure(ObservabilityOptions options)
        {
            if (_initialized)
            {
                return;
            }

            var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "logs");
            Directory.CreateDirectory(logDirectory);

            var enableEfSqlLogging = options != null && options.EnableEfSqlLogging;
            EfSqlLogger.Configure(enableEfSqlLogging);

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override(EfSqlLogger.LoggerName, enableEfSqlLogging ? LogEventLevel.Debug : LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "EImece")
                .Enrich.WithMachineName()
                .WriteTo.File(
                    formatter: new Serilog.Formatting.Compact.CompactJsonFormatter(),
                    path: Path.Combine(logDirectory, "structured-.json"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    shared: true);

            if (enableEfSqlLogging)
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e =>
                        e.Properties.ContainsKey("SourceContext")
                        && e.Properties["SourceContext"].ToString().IndexOf(EfSqlLogger.LoggerName, StringComparison.Ordinal) >= 0)
                    .WriteTo.File(
                        path: Path.Combine(logDirectory, "ef-sql-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        shared: true,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}"));
            }

            Log.Logger = loggerConfiguration.CreateLogger();

            _initialized = true;
        }

        public static void EnrichFromHttpContext()
        {
            var context = HttpContext.Current;
            if (context == null)
            {
                return;
            }

            LogContext.PushProperty("CorrelationId", CorrelationIdContext.Ensure());
            LogContext.PushProperty("RequestId", context.Items["RequestId"]);
            LogContext.PushProperty("ClientIp", context.Request.UserHostAddress);
            LogContext.PushProperty("RequestPath", context.Request.Url?.AbsolutePath);
            LogContext.PushProperty("HttpMethod", context.Request.HttpMethod);

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                LogContext.PushProperty("UserId", context.User.Identity.Name);
            }
        }

        public static void LogRequestCompleted(long durationMs, int statusCode)
        {
            Log.ForContext("ExecutionTimeMs", durationMs)
                .ForContext("StatusCode", statusCode)
                .Write(LogEventLevel.Information, "HTTP request completed");
        }

        public static void LogException(Exception exception, string message)
        {
            Log.ForContext("ExceptionType", exception.GetType().FullName)
                .Error(exception, SensitiveDataMasker.Mask(message));
        }
    }
}
