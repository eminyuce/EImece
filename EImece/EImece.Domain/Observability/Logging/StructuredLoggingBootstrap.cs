using EImece.Domain.Observability.Configuration;
using NLog;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.IO;

namespace EImece.Domain.Observability.Logging
{
    public static class StructuredLoggingBootstrap
    {
        private const string CorrelationIdProperty = "CorrelationId";
        private const string TraceIdProperty = "TraceId";
        private const string SpanIdProperty = "SpanId";

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

            // Same writable root as uploads (media/) — one IIS ACL for images + logs.
            var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "media", "logs");
            Directory.CreateDirectory(logDirectory);

            var enableEfSqlLogging = options != null && options.EnableEfSqlLogging;
            EfSqlLogger.Configure(enableEfSqlLogging, options != null && options.EnableEfTelemetry);

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "EImece")
                .Enrich.WithMachineName();

            Log.Logger = loggerConfiguration.CreateLogger();

            _initialized = true;
        }

        public static void EnrichFromActivity(Activity activity = null)
        {
            activity = activity ?? Activity.Current;
            var correlationId = CorrelationIdContext.Ensure();
            LogContext.PushProperty(CorrelationIdProperty, correlationId);
            ScopeContext.PushProperty(CorrelationIdProperty, correlationId);

            if (activity == null)
            {
                return;
            }

            var traceId = activity.TraceId.ToString();
            var spanId = activity.SpanId.ToString();
            LogContext.PushProperty(TraceIdProperty, traceId);
            LogContext.PushProperty(SpanIdProperty, spanId);
            ScopeContext.PushProperty(TraceIdProperty, traceId);
            ScopeContext.PushProperty(SpanIdProperty, spanId);
        }

        public static void LogRequestCompleted(long durationMs, int statusCode)
        {
            Log.ForContext("ExecutionTimeMs", durationMs)
                .ForContext("StatusCode", statusCode)
                .ForContext(CorrelationIdProperty, CorrelationIdContext.Current)
                .ForContext(TraceIdProperty, Activity.Current?.TraceId.ToString())
                .ForContext(SpanIdProperty, Activity.Current?.SpanId.ToString())
                .Write(LogEventLevel.Information, "HTTP request completed");
        }

        public static void LogException(Exception exception, string message)
        {
            Log.ForContext("ExceptionType", exception.GetType().FullName)
                .ForContext(CorrelationIdProperty, CorrelationIdContext.Current)
                .ForContext(TraceIdProperty, Activity.Current?.TraceId.ToString())
                .ForContext(SpanIdProperty, Activity.Current?.SpanId.ToString())
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
