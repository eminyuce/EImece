using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Filters
{
    /// <summary>
    /// Result returned by rate limiter checking whether a request is allowed.
    /// </summary>
    public class RateLimitCheckResult
    {
        public bool IsAllowed { get; set; }
        public int Remaining { get; set; }
        public int Limit { get; set; }
        public int RetryAfterSeconds { get; set; }
    }

    /// <summary>
    /// Lightweight, thread-safe in-memory sliding-window rate limiter.
    /// Tracks request timestamps per key in a ConcurrentDictionary and purges expired entries periodically.
    /// </summary>
    public static class InMemoryRateLimiter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private class ClientRequestRecord
        {
            public readonly object SyncRoot = new object();
            public readonly List<DateTime> RequestTimestampsUtc = new List<DateTime>();
            public DateTime LastSeenUtc = DateTime.UtcNow;
        }

        private static readonly ConcurrentDictionary<string, ClientRequestRecord> Store =
            new ConcurrentDictionary<string, ClientRequestRecord>(StringComparer.OrdinalIgnoreCase);

        private static DateTime _lastCleanupUtc = DateTime.UtcNow;
        private static readonly object CleanupLock = new object();
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Checks whether a request for the given key is allowed under the sliding time window limit.
        /// </summary>
        /// <param name="key">Rate limit partition key (e.g. "login:192.168.1.1").</param>
        /// <param name="limit">Maximum number of requests permitted within the window.</param>
        /// <param name="window">Sliding window time duration (e.g. 15 minutes).</param>
        /// <returns>RateLimitCheckResult with allowed status, remaining quota, and retry-after seconds.</returns>
        public static RateLimitCheckResult Check(string key, int limit, TimeSpan window)
        {
            if (string.IsNullOrWhiteSpace(key) || limit <= 0)
            {
                return new RateLimitCheckResult { IsAllowed = true, Remaining = 0, Limit = 0, RetryAfterSeconds = 0 };
            }

            var now = DateTime.UtcNow;
            var record = Store.GetOrAdd(key, _ => new ClientRequestRecord());

            lock (record.SyncRoot)
            {
                record.LastSeenUtc = now;
                var windowStart = now - window;

                // Remove timestamps older than the sliding window
                record.RequestTimestampsUtc.RemoveAll(t => t < windowStart);

                if (record.RequestTimestampsUtc.Count >= limit)
                {
                    var oldestInWindow = record.RequestTimestampsUtc.FirstOrDefault();
                    var retryAfter = oldestInWindow != default(DateTime)
                        ? (int)Math.Ceiling((oldestInWindow + window - now).TotalSeconds)
                        : (int)window.TotalSeconds;

                    if (retryAfter < 1) retryAfter = 1;

                    return new RateLimitCheckResult
                    {
                        IsAllowed = false,
                        Limit = limit,
                        Remaining = 0,
                        RetryAfterSeconds = retryAfter
                    };
                }

                // Allowed: record this request timestamp
                record.RequestTimestampsUtc.Add(now);
                int remaining = Math.Max(0, limit - record.RequestTimestampsUtc.Count);

                // Check for background cleanup periodically
                TryTriggerCleanup(now);

                return new RateLimitCheckResult
                {
                    IsAllowed = true,
                    Limit = limit,
                    Remaining = remaining,
                    RetryAfterSeconds = 0
                };
            }
        }

        /// <summary>
        /// Cleans up stale entries to prevent memory growth over time.
        /// </summary>
        private static void TryTriggerCleanup(DateTime now)
        {
            if (now - _lastCleanupUtc < CleanupInterval)
            {
                return;
            }

            if (!System.Threading.Monitor.TryEnter(CleanupLock))
            {
                return;
            }

            try
            {
                _lastCleanupUtc = now;
                var maxStaleCutoff = now - TimeSpan.FromHours(1);

                foreach (var pair in Store)
                {
                    if (pair.Value.LastSeenUtc < maxStaleCutoff)
                    {
                        Store.TryRemove(pair.Key, out _);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred during in-memory rate limiter cleanup.");
            }
            finally
            {
                System.Threading.Monitor.Exit(CleanupLock);
            }
        }

        /// <summary>
        /// Resets the store (useful for unit testing and maintenance).
        /// </summary>
        public static void Reset()
        {
            Store.Clear();
        }
    }
}
