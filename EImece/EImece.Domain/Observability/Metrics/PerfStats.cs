using EImece.Domain.DependencyInjection;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace EImece.Domain.Observability.Metrics
{
    /// <summary>
    /// Snapshot representing aggregated performance statistics for a single timed metric.
    /// </summary>
    public sealed class PerfStatSnapshot
    {
        public string Name { get; set; }
        public long Count { get; set; }
        public double SumMs { get; set; }
        public double AvgMs { get; set; }
        public double MinMs { get; set; }
        public double MaxMs { get; set; }
        public double LastMs { get; set; }
        public DateTime LastUtc { get; set; }
    }

    /// <summary>
    /// Thread-safe in-memory store for [Timed] execution performance statistics with configurable retention.
    /// Retention is driven by the "PerfStatsRetentionHours" system setting (default: 24h).
    /// If retention is set to 0 hours, metric collection is disabled.
    /// If set to any positive integer (e.g. 4 hours), stats are retained and aggregated for the last N hours.
    /// Scope: Manages ONLY its own in-memory timer stats; does not touch application caching or HttpRuntime.Cache.
    /// </summary>
    public static class PerfStats
    {
        public const int DefaultRetentionHours = 1;

        private static readonly ConcurrentDictionary<string, PerfStatEntry> Entries =
            new ConcurrentDictionary<string, PerfStatEntry>(StringComparer.Ordinal);

        private static long _lastEvictionTicks = DateTime.UtcNow.Ticks;

        /// <summary>
        /// Optional delegate to override or mock the retention hours provider (e.g. in unit tests).
        /// When null, resolves dynamically from ISettingService / AppConfig.
        /// </summary>
        public static Func<int> RetentionHoursProvider { get; set; }

        /// <summary>
        /// Gets the current retention hours setting. Returns 0 if disabled, or positive integer (e.g. 4, 24).
        /// </summary>
        public static int GetRetentionHours()
        {
            if (RetentionHoursProvider != null)
            {
                return Math.Max(0, RetentionHoursProvider());
            }

            return ResolveConfiguredRetentionHours();
        }

        /// <summary>
        /// Returns true if performance statistics collection is enabled (retention hours > 0).
        /// </summary>
        public static bool IsEnabled => GetRetentionHours() > 0;

        /// <summary>
        /// Records an execution sample. Thread-safe.
        /// If retention hours <= 0, collection is disabled and no sample is recorded.
        /// Stale entries (> retention period since last sample) have their counters reset before applying the new sample.
        /// </summary>
        /// <param name="name">Metric name, e.g. "service.products.search" or "app.home.index".</param>
        /// <param name="elapsedMs">Elapsed duration in milliseconds.</param>
        public static void Record(string name, double elapsedMs)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var retentionHours = GetRetentionHours();
            if (retentionHours <= 0)
            {
                // Collection disabled (0 hours)
                if (!Entries.IsEmpty)
                {
                    Entries.Clear();
                }
                return;
            }

            var retentionPeriod = TimeSpan.FromHours(retentionHours);
            var now = DateTime.UtcNow;
            EvictExpiredLazy(now, retentionPeriod);

            Entries.AddOrUpdate(
                name,
                k =>
                {
                    var entry = new PerfStatEntry(k);
                    entry.Record(elapsedMs, now);
                    return entry;
                },
                (k, existing) =>
                {
                    existing.Record(elapsedMs, now, retentionPeriod);
                    return existing;
                });
        }

        /// <summary>
        /// Returns a snapshot of all active timer statistics within the configured retention window, sorted by AvgMs descending.
        /// Automatically evicts expired entries older than the retention period.
        /// </summary>
        public static List<PerfStatSnapshot> Snapshot()
        {
            var retentionHours = GetRetentionHours();
            if (retentionHours <= 0)
            {
                Entries.Clear();
                return new List<PerfStatSnapshot>();
            }

            var retentionPeriod = TimeSpan.FromHours(retentionHours);
            var now = DateTime.UtcNow;
            EvictExpiredFull(now, retentionPeriod);

            var list = new List<PerfStatSnapshot>();
            foreach (var kvp in Entries)
            {
                var snap = kvp.Value.GetSnapshot(now, retentionPeriod);
                if (snap != null)
                {
                    list.Add(snap);
                }
            }

            list.Sort((a, b) => b.AvgMs.CompareTo(a.AvgMs));
            return list;
        }

        /// <summary>
        /// Clears ONLY this timer stats dictionary immediately.
        /// Does not affect HttpRuntime.Cache, output cache, or any application cache.
        /// </summary>
        public static void Clear()
        {
            Entries.Clear();
        }

        private static int ResolveConfiguredRetentionHours()
        {
            try
            {
                // 1. Try reading dynamic cached setting from ISettingService
                var sp = DomainServiceProvider.Instance;
                if (sp != null)
                {
                    var settingService = sp.GetService(typeof(ISettingService)) as ISettingService;
                    if (settingService != null)
                    {
                        var val = settingService.GetSettingByKey(Constants.PerfStatsRetentionHours);
                        if (!string.IsNullOrWhiteSpace(val) && int.TryParse(val, out var parsedDbHours))
                        {
                            return Math.Max(0, parsedDbHours);
                        }
                    }
                }
            }
            catch
            {
                // Ignore resolution errors (e.g. during test startup before DI is configured)
            }

            // 2. Fallback to AppConfig / Web.config AppSettings / Default
            return Math.Max(0, AppConfig.GetConfigInt(Constants.PerfStatsRetentionHours, DefaultRetentionHours));
        }

        private static void EvictExpiredLazy(DateTime nowUtc, TimeSpan retentionPeriod)
        {
            var lastTicks = Volatile.Read(ref _lastEvictionTicks);
            var nowTicks = nowUtc.Ticks;
            if (nowTicks - lastTicks < TimeSpan.TicksPerMinute)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref _lastEvictionTicks, nowTicks, lastTicks) == lastTicks)
            {
                EvictExpiredFull(nowUtc, retentionPeriod);
            }
        }

        private static void EvictExpiredFull(DateTime nowUtc, TimeSpan retentionPeriod)
        {
            foreach (var kvp in Entries)
            {
                if (kvp.Value.IsExpired(nowUtc, retentionPeriod))
                {
                    Entries.TryRemove(kvp.Key, out _);
                }
            }
        }

        private sealed class PerfStatEntry
        {
            private readonly object _lock = new object();
            private readonly string _name;
            private long _count;
            private double _sumMs;
            private double _minMs;
            private double _maxMs;
            private double _lastMs;
            private DateTime _lastUtc;

            public PerfStatEntry(string name)
            {
                _name = name;
            }

            public bool IsExpired(DateTime nowUtc, TimeSpan retention)
            {
                lock (_lock)
                {
                    return _count == 0 || (nowUtc - _lastUtc) > retention;
                }
            }

            public void Record(double elapsedMs, DateTime nowUtc, TimeSpan? retention = null)
            {
                lock (_lock)
                {
                    if (retention.HasValue && _count > 0 && (nowUtc - _lastUtc) > retention.Value)
                    {
                        // Reset stale entry counters before applying new sample
                        _count = 0;
                        _sumMs = 0;
                        _minMs = 0;
                        _maxMs = 0;
                    }

                    if (_count == 0)
                    {
                        _count = 1;
                        _sumMs = elapsedMs;
                        _minMs = elapsedMs;
                        _maxMs = elapsedMs;
                        _lastMs = elapsedMs;
                        _lastUtc = nowUtc;
                    }
                    else
                    {
                        _count++;
                        _sumMs += elapsedMs;
                        if (elapsedMs < _minMs) _minMs = elapsedMs;
                        if (elapsedMs > _maxMs) _maxMs = elapsedMs;
                        _lastMs = elapsedMs;
                        _lastUtc = nowUtc;
                    }
                }
            }

            public PerfStatSnapshot GetSnapshot(DateTime nowUtc, TimeSpan retention)
            {
                lock (_lock)
                {
                    if (_count == 0 || (nowUtc - _lastUtc) > retention)
                    {
                        return null;
                    }

                    return new PerfStatSnapshot
                    {
                        Name = _name,
                        Count = _count,
                        SumMs = _sumMs,
                        AvgMs = _count > 0 ? _sumMs / _count : 0,
                        MinMs = _minMs,
                        MaxMs = _maxMs,
                        LastMs = _lastMs,
                        LastUtc = _lastUtc
                    };
                }
            }
        }
    }
}
