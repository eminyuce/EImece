using EImece.Domain.Observability.Configuration;
using NLog;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System;
using System.Diagnostics;
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
            EfSqlLogger.Configure(enableEfSqlLogging, options != null && options.EnableEfTelemetry);

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

            var correlationId = CorrelationIdContext.Ensure();
            var activity = Activity.Current;
            var traceId = activity?.TraceId.ToString();
            var spanId = activity?.SpanId.ToString();

            LogContext.PushProperty("CorrelationId", correlationId);
            LogContext.PushProperty("RequestId", context.Items["RequestId"]);
            LogContext.PushProperty("ClientIp", context.Request.UserHostAddress);
            LogContext.PushProperty("RequestPath", context.Request.Url?.AbsolutePath);
            LogContext.PushProperty("HttpMethod", context.Request.HttpMethod);

            if (!string.IsNullOrEmpty(traceId))
            {
                LogContext.PushProperty("TraceId", traceId);
            }

            if (!string.IsNullOrEmpty(spanId))
            {
                LogContext.PushProperty("SpanId", spanId);
            }

            // NLog scope properties for layouts that read ${scopeproperty:item=...}
            ScopeContext.PushProperty("CorrelationId", correlationId);
            if (!string.IsNullOrEmpty(traceId))
            {
                ScopeContext.PushProperty("TraceId", traceId);
            }

            if (!string.IsNullOrEmpty(spanId))
            {
                ScopeContext.PushProperty("SpanId", spanId);
            }

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                LogContext.PushProperty("UserId", context.User.Identity.Name);
            }
        }

        public static void EnrichFromActivity(Activity activity = null)
        {
            activity = activity ?? Activity.Current;
            var correlationId = CorrelationIdContext.Ensure();
            LogContext.PushProperty("CorrelationId", correlationId);
            ScopeContext.PushProperty("CorrelationId", correlationId);

            if (activity == null)
            {
                return;
            }

            var traceId = activity.TraceId.ToString();
            var spanId = activity.SpanId.ToString();
            LogContext.PushProperty("TraceId", traceId);
            LogContext.PushProperty("SpanId", spanId);
            ScopeContext.PushProperty("TraceId", traceId);
            ScopeContext.PushProperty("SpanId", spanId);
        }

        public static void LogRequestCompleted(long durationMs, int statusCode)
        {
            Log.ForContext("ExecutionTimeMs", durationMs)
                .ForContext("StatusCode", statusCode)
                .ForContext("CorrelationId", CorrelationIdContext.Current)
                .ForContext("TraceId", Activity.Current?.TraceId.ToString())
                .ForContext("SpanId", Activity.Current?.SpanId.ToString())
                .Write(LogEventLevel.Information, "HTTP request completed");
        }

        public static void LogException(Exception exception, string message)
        {
            Log.ForContext("ExceptionType", exception.GetType().FullName)
                .ForContext("CorrelationId", CorrelationIdContext.Current)
                .ForContext("TraceId", Activity.Current?.TraceId.ToString())
                .ForContext("SpanId", Activity.Current?.SpanId.ToString())
                .Error(exception, SensitiveDataMasker.Mask(message));
        }

        public static void CloseAndFlush()
        {
            try
            {
                Log.CloseAndFlush();
            }
            catch
            {
                // Best-effort during AppDomain recycle.
            }
        }
    }
}
