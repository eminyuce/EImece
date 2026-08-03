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

            // Configure Serilog with no file sinks so application-level logs are effectively disabled.
            // Entity Framework SQL will still be routed to NLog (and from there to the database) via EfSqlLogger.
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "EImece")
                .Enrich.WithMachineName();

            // Do not write to any file sinks here — keep Serilog active but silent. EF SQL logging is handled
            // by EfSqlLogger which writes to NLog (database target) and also to Serilog context; leaving Serilog
            // without sinks ensures no file logs are produced.

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
