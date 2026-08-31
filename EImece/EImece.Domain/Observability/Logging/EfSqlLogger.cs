using EImece.Domain.Observability.Metrics;
using EImece.Domain.Observability.Telemetry;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace EImece.Domain.Observability.Logging
{
    /// <summary>
    /// Routes Entity Framework 6 <see cref="System.Data.Entity.Database.Log"/> output through MEL.
    /// Optionally records light Activities/metrics without exporting full SQL text in production.
    /// </summary>
    public static class EfSqlLogger
    {
        public const string LoggerName = "EntityFramework.Sql";

        private static readonly Regex CommandTypePattern = new Regex(
            @"^\s*(SELECT|INSERT|UPDATE|DELETE|EXEC|EXECUTE|MERGE)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(1));

        private static readonly object Sync = new object();
        private static ILogger _logger;
        private static bool _enabled;
        private static bool _telemetryEnabled = true;

        public static bool IsEnabled => _enabled;

        public static void Configure(ILoggerFactory loggerFactory, bool enabled, bool telemetryEnabled = true)
        {
            lock (Sync)
            {
                _logger = loggerFactory?.CreateLogger(LoggerName);
                _enabled = enabled;
                _telemetryEnabled = telemetryEnabled;
            }
        }

        public static void Attach(System.Data.Entity.DbContext context)
        {
            if (!_enabled || context == null)
            {
                return;
            }

            context.Database.Log = Write;
        }

        public static void Write(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return;
            }

            var trimmed = sql.TrimEnd();
            var operation = ResolveOperation(trimmed);

            if (_telemetryEnabled && !string.IsNullOrEmpty(operation))
            {
                OpenTelemetryMetrics.RecordDatabaseOperation(operation, durationMs: 0, success: true);

                var activity = OpenTelemetryBootstrap.ActivitySource?.StartActivity(
                    "db." + operation.ToLowerInvariant(),
                    ActivityKind.Client);
                if (activity != null)
                {
                    activity.SetTag(ActivityTags.DbSystem, "mssql");
                    activity.SetTag(ActivityTags.DbOperation, operation);
                    activity.SetTag(ActivityTags.CorrelationId, CorrelationIdContext.Current);
                    activity.Dispose();
                }
            }

            if (!_enabled || _logger == null || !_logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            var message = SensitiveDataMasker.Mask(trimmed);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            _logger.LogDebug("{Sql}", message);
        }

        private static string ResolveOperation(string sql)
        {
            var match = CommandTypePattern.Match(sql);
            if (!match.Success)
            {
                return null;
            }

            return match.Groups[1].Value.ToUpperInvariant();
        }
    }
}
