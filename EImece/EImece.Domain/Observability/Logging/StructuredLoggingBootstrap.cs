using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EImece.Domain.Observability.Logging
{
    /// <summary>
    /// Request-scoped enrichment and cross-cutting log helpers built on Microsoft.Extensions.Logging.
    /// Reuses existing CorrelationId / Activity trace context — no second correlation system.
    /// </summary>
    public static class StructuredLoggingBootstrap
    {
        private const string CorrelationIdProperty = "CorrelationId";
        private const string TraceIdProperty = "TraceId";
        private const string SpanIdProperty = "SpanId";

        private static Microsoft.Extensions.Logging.ILogger _pipelineLogger;
        private static bool _initialized;

        public static void Configure(Observability.Configuration.ObservabilityOptions options)
        {
            if (_initialized)
            {
                return;
            }

            var factory = LoggingBootstrap.Configure(LoggingOptions.FromAppConfig());
            _pipelineLogger = factory.CreateLogger("EImece.RequestPipeline");

            var enableEfSqlLogging = options != null && options.EnableEfSqlLogging;
            EfSqlLogger.Configure(factory, enableEfSqlLogging, options != null && options.EnableEfTelemetry);

            _initialized = true;
        }

        public static void EnrichFromActivity(Activity activity = null)
        {
            activity = activity ?? Activity.Current;
            var correlationId = CorrelationIdContext.Ensure();
            PushScopeProperties(correlationId, activity);
        }

        public static void LogRequestCompleted(long elapsedMs, int statusCode, string httpMethod = null, string requestPath = null, string userId = null)
        {
            if (_pipelineLogger == null || !_pipelineLogger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            using (BeginRequestScope())
            {
                _pipelineLogger.LogDebug(
                    "HTTP request completed {HttpMethod} {RequestPath} {StatusCode} {ElapsedMs} {UserId}",
                    httpMethod,
                    requestPath,
                    statusCode,
                    elapsedMs,
                    userId);
            }
        }

        public static void LogException(Exception exception, string message)
        {
            if (exception == null || _pipelineLogger == null)
            {
                return;
            }

            using (BeginRequestScope())
            {
                _pipelineLogger.LogError(
                    exception,
                    "{Message}",
                    SensitiveDataMasker.Mask(message));
            }
        }

        public static IDisposable BeginRequestScope()
        {
            var activity = Activity.Current;
            var correlationId = CorrelationIdContext.Current ?? CorrelationIdContext.Ensure();
            var state = new Dictionary<string, object>
            {
                [CorrelationIdProperty] = correlationId,
            };

            if (activity != null)
            {
                state[TraceIdProperty] = activity.TraceId.ToString();
                state[SpanIdProperty] = activity.SpanId.ToString();
            }

            PushNLogScopeProperties(state);
            return _pipelineLogger?.BeginScope(state) ?? NullScope.Instance;
        }

        public static void CloseAndFlush()
        {
            LoggingBootstrap.FlushAndShutdown();
            _pipelineLogger = null;
            _initialized = false;
        }

        private static void PushScopeProperties(string correlationId, Activity activity)
        {
            var state = new Dictionary<string, object>
            {
                [CorrelationIdProperty] = correlationId,
            };

            if (activity != null)
            {
                state[TraceIdProperty] = activity.TraceId.ToString();
                state[SpanIdProperty] = activity.SpanId.ToString();
            }

            PushNLogScopeProperties(state);
        }

        private static void PushNLogScopeProperties(IReadOnlyDictionary<string, object> state)
        {
            foreach (var pair in state)
            {
                if (pair.Value != null)
                {
                    NLog.ScopeContext.PushProperty(pair.Key, pair.Value);
                }
            }
        }

        private sealed class NullScope : IDisposable
        {
            internal static readonly NullScope Instance = new NullScope();
            public void Dispose() { }
        }
    }
}
