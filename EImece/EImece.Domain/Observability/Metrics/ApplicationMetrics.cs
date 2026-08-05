using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Observability.Metrics
{
    public interface IApplicationMetrics
    {
        void RecordRequest(string name, long durationMilliseconds, bool success);

        void RecordRequest(string name, long durationMilliseconds, bool success, string httpMethod, int statusCode);

        void RecordHttpCall(string url, string method, int statusCode, long durationMilliseconds, int retryCount);

        void RecordDatabaseQuery(string operation, long durationMilliseconds, bool success);

        IReadOnlyDictionary<string, MetricSnapshot> GetSnapshots();
    }

    public sealed class MetricSnapshot
    {
        public long Count { get; set; }

        public long ErrorCount { get; set; }

        public double AverageDurationMs { get; set; }

        public long P95DurationMs { get; set; }
    }

    public sealed class ApplicationMetrics : IApplicationMetrics
    {
        private readonly ConcurrentDictionary<string, MetricAccumulator> _metrics = new ConcurrentDictionary<string, MetricAccumulator>(StringComparer.OrdinalIgnoreCase);

        public void RecordRequest(string name, long durationMilliseconds, bool success)
        {
            RecordRequest(name, durationMilliseconds, success, null, success ? 200 : 500);
        }

        public void RecordRequest(string name, long durationMilliseconds, bool success, string httpMethod, int statusCode)
        {
            Record("request:" + OpenTelemetryMetrics.NormalizeRoute(name), durationMilliseconds, success);
            OpenTelemetryMetrics.RecordServerRequest(httpMethod ?? "GET", name, statusCode, durationMilliseconds);
        }

        public void RecordHttpCall(string url, string method, int statusCode, long durationMilliseconds, int retryCount)
        {
            var operation = OpenTelemetryMetrics.NormalizeRoute(url);
            var key = "http:" + OpenTelemetryMetrics.NormalizeMethod(method) + ":" + statusCode;
            Record(key, durationMilliseconds, statusCode >= 200 && statusCode < 500);
            OpenTelemetryMetrics.RecordClientRequest(method, operation, statusCode, durationMilliseconds, retryCount);
        }

        public void RecordDatabaseQuery(string operation, long durationMilliseconds, bool success)
        {
            var normalized = OpenTelemetryMetrics.NormalizeRoute(operation);
            Record("db:" + normalized, durationMilliseconds, success);
            OpenTelemetryMetrics.RecordDatabaseOperation(normalized, durationMilliseconds, success);
        }

        public IReadOnlyDictionary<string, MetricSnapshot> GetSnapshots()
        {
            return _metrics.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToSnapshot(),
                StringComparer.OrdinalIgnoreCase);
        }

        private void Record(string key, long durationMilliseconds, bool success)
        {
            _metrics.AddOrUpdate(
                key,
                _ => new MetricAccumulator().Add(durationMilliseconds, success),
                (_, accumulator) => accumulator.Add(durationMilliseconds, success));
        }

        private sealed class MetricAccumulator
        {
            private readonly List<long> _durations = new List<long>();
            private long _errorCount;

            public MetricAccumulator Add(long durationMilliseconds, bool success)
            {
                lock (_durations)
                {
                    _durations.Add(durationMilliseconds);
                    if (!success)
                    {
                        _errorCount++;
                    }
                }

                return this;
            }

            public MetricSnapshot ToSnapshot()
            {
                lock (_durations)
                {
                    if (_durations.Count == 0)
                    {
                        return new MetricSnapshot();
                    }

                    var ordered = _durations.OrderBy(x => x).ToList();
                    var p95Index = Math.Max(0, (int)Math.Ceiling(ordered.Count * 0.95) - 1);

                    return new MetricSnapshot
                    {
                        Count = ordered.Count,
                        ErrorCount = _errorCount,
                        AverageDurationMs = ordered.Average(),
                        P95DurationMs = ordered[p95Index]
                    };
                }
            }
        }
    }
}
