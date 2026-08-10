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

        /// <summary>
        /// Records a Controller or Service method invocation for in-process P90/P95/P99 snapshots
        /// and OpenTelemetry histograms.
        /// </summary>
        void RecordMethod(string layer, string typeName, string methodName, long durationMilliseconds, bool success);

        IReadOnlyDictionary<string, MetricSnapshot> GetSnapshots();
    }

    public sealed class MetricSnapshot
    {
        public long Count { get; set; }

        public long ErrorCount { get; set; }

        /// <summary>
        /// Number of recent samples retained for percentile calculation (bounded window).
        /// </summary>
        public int SampleWindowSize { get; set; }

        public double AverageDurationMs { get; set; }

        public long P90DurationMs { get; set; }

        public long P95DurationMs { get; set; }

        public long P99DurationMs { get; set; }
    }

    /// <summary>
    /// Thread-safe in-process latency metrics with a bounded ring buffer per key.
    /// Percentiles (P90/P95/P99) are computed from the most recent samples so memory stays
    /// predictable under high concurrency, while <see cref="MetricSnapshot.Count"/> tracks
    /// the lifetime invocation count.
    /// </summary>
    public sealed class ApplicationMetrics : IApplicationMetrics
    {
        /// <summary>
        /// Recent samples retained per metric key for percentile calculation.
        /// 2048 × 8 bytes ≈ 16 KB per key; enough for stable tail percentiles.
        /// </summary>
        public const int DefaultSampleCapacity = 2048;

        private readonly ConcurrentDictionary<string, MetricAccumulator> _metrics =
            new ConcurrentDictionary<string, MetricAccumulator>(StringComparer.OrdinalIgnoreCase);

        private readonly int _sampleCapacity;

        public ApplicationMetrics()
            : this(DefaultSampleCapacity)
        {
        }

        public ApplicationMetrics(int sampleCapacity)
        {
            if (sampleCapacity < 16)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleCapacity), "Sample capacity must be at least 16.");
            }

            _sampleCapacity = sampleCapacity;
        }

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

        public void RecordMethod(string layer, string typeName, string methodName, long durationMilliseconds, bool success)
        {
            var safeLayer = string.IsNullOrWhiteSpace(layer) ? "unknown" : layer.Trim().ToLowerInvariant();
            var safeType = OpenTelemetryMetrics.NormalizeTypeName(typeName);
            var safeMethod = OpenTelemetryMetrics.NormalizeMethodName(methodName);
            var key = safeLayer + ":" + safeType + "." + safeMethod;

            Record(key, durationMilliseconds, success);
            OpenTelemetryMetrics.RecordMethodDuration(safeLayer, safeType, safeMethod, durationMilliseconds, success);
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
            if (durationMilliseconds < 0)
            {
                durationMilliseconds = 0;
            }

            _metrics.AddOrUpdate(
                key,
                _ => new MetricAccumulator(_sampleCapacity).Add(durationMilliseconds, success),
                (_, accumulator) => accumulator.Add(durationMilliseconds, success));
        }

        private sealed class MetricAccumulator
        {
            private readonly long[] _samples;
            private readonly object _gate = new object();
            private int _filled;
            private int _next;
            private long _totalCount;
            private long _errorCount;
            private long _sumDurationMs;

            public MetricAccumulator(int capacity)
            {
                _samples = new long[capacity];
            }

            public MetricAccumulator Add(long durationMilliseconds, bool success)
            {
                lock (_gate)
                {
                    _samples[_next] = durationMilliseconds;
                    _next = (_next + 1) % _samples.Length;
                    if (_filled < _samples.Length)
                    {
                        _filled++;
                    }

                    _totalCount++;
                    _sumDurationMs += durationMilliseconds;
                    if (!success)
                    {
                        _errorCount++;
                    }
                }

                return this;
            }

            public MetricSnapshot ToSnapshot()
            {
                lock (_gate)
                {
                    if (_filled == 0 || _totalCount == 0)
                    {
                        return new MetricSnapshot();
                    }

                    var window = new long[_filled];
                    if (_filled < _samples.Length)
                    {
                        Array.Copy(_samples, 0, window, 0, _filled);
                    }
                    else
                    {
                        // Ring is full: logical order is [_next .. end) + [0 .. _next)
                        var tail = _samples.Length - _next;
                        Array.Copy(_samples, _next, window, 0, tail);
                        Array.Copy(_samples, 0, window, tail, _next);
                    }

                    Array.Sort(window);

                    return new MetricSnapshot
                    {
                        Count = _totalCount,
                        ErrorCount = _errorCount,
                        SampleWindowSize = _filled,
                        AverageDurationMs = _totalCount == 0 ? 0d : (double)_sumDurationMs / _totalCount,
                        P90DurationMs = LatencyPercentiles.NearestRank(window, 0.90),
                        P95DurationMs = LatencyPercentiles.NearestRank(window, 0.95),
                        P99DurationMs = LatencyPercentiles.NearestRank(window, 0.99)
                    };
                }
            }
        }
    }

    /// <summary>
    /// Nearest-rank percentile over a pre-sorted ascending sample window.
    /// </summary>
    public static class LatencyPercentiles
    {
        public static long NearestRank(IReadOnlyList<long> sortedAscending, double percentile)
        {
            if (sortedAscending == null || sortedAscending.Count == 0)
            {
                return 0;
            }

            if (percentile <= 0d)
            {
                return sortedAscending[0];
            }

            if (percentile >= 1d)
            {
                return sortedAscending[sortedAscending.Count - 1];
            }

            var rank = (int)Math.Ceiling(percentile * sortedAscending.Count) - 1;
            if (rank < 0)
            {
                rank = 0;
            }

            if (rank >= sortedAscending.Count)
            {
                rank = sortedAscending.Count - 1;
            }

            return sortedAscending[rank];
        }
    }
}
