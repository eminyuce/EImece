using EImece.Domain.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace EImece.Domain.Caching
{
    public class MemoryCacheProvider : IEimeceCacheProvider
    {
        private readonly ILogger<MemoryCacheProvider> _logger;

        private const string PhysicalKeyPrefix = "Memory:";
        // Sole remaining use of the System.Runtime.Caching default host. Application code must
        // go through IEimeceCacheProvider so this adapter can be replaced later.
        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly IHttpRuntimeCacheClearer _httpRuntimeCacheClearer;

        public MemoryCacheProvider(ILogger<MemoryCacheProvider> logger, IHttpRuntimeCacheClearer httpRuntimeCacheClearer = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpRuntimeCacheClearer = httpRuntimeCacheClearer;
        }

        public bool Get<T>(string key, out T value)
        {
            if (AppConfig.IsCacheActive)
            {
                var keyNew = ToPhysicalKey(key);
                var sw = Stopwatch.StartNew();
                var cached = _cache[keyNew];
                if (cached == null)
                {
                    CacheDiagnostics.RecordMiss(keyNew);
                    value = default(T);
                    return false;
                }

                // GetOrAdd stores Lazy<T> for single-flight; Set stores T directly.
                if (cached is Lazy<T> lazy)
                {
                    var resolved = lazy.Value;
                    sw.Stop();
                    CacheDiagnostics.RecordHit(keyNew);
                    CacheDiagnostics.RecordLookupDuration(keyNew, true, sw.ElapsedTicks);
                    value = resolved;
                    return true;
                }

                if (cached is T typed)
                {
                    sw.Stop();
                    CacheDiagnostics.RecordHit(keyNew);
                    CacheDiagnostics.RecordLookupDuration(keyNew, true, sw.ElapsedTicks);
                    value = typed;
                    return true;
                }

                CacheDiagnostics.RecordMiss(keyNew);
                value = default(T);
                return false;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public T GetOrAdd<T>(string key, Func<T> valueFactory, int duration)
        {
            return GetOrAdd(key, valueFactory, CachePolicy.Absolute(duration));
        }

        public T GetOrAdd<T>(string key, Func<T> valueFactory, CachePolicy policy)
        {
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));
            if (policy == null) throw new ArgumentNullException(nameof(policy));

            if (!AppConfig.IsCacheActive)
            {
                return valueFactory();
            }

            var keyNew = ToPhysicalKey(key);

            // Store a Lazy<T> and publish it with AddOrGetExisting so that concurrent callers
            // resolve the SAME Lazy instance; the value factory therefore executes exactly once
            // (single-flight), preventing the cache stampede of a naive get-then-set.
            var newLazy = new Lazy<T>(valueFactory, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            var cachePolicy = CreateItemPolicy(policy);
            var existing = _cache.AddOrGetExisting(keyNew, newLazy, cachePolicy);
            var added = existing == null;
            var winner = existing as Lazy<T> ?? newLazy;

            var sw = Stopwatch.StartNew();
            try
            {
                var value = winner.Value;
                sw.Stop();
                if (added)
                {
                    CacheDiagnostics.RecordSet(keyNew, typeof(T), policy);
                    CacheDiagnostics.RecordMiss(keyNew);
                    CacheDiagnostics.RecordLookupDuration(keyNew, false, sw.ElapsedTicks);
                }
                else
                {
                    CacheDiagnostics.RecordHit(keyNew);
                    CacheDiagnostics.RecordLookupDuration(keyNew, true, sw.ElapsedTicks);
                }

                return value;
            }
            catch
            {
                // Never cache a faulted factory result.
                _cache.Remove(keyNew);
                if (added)
                {
                    CacheDiagnostics.RecordMiss(keyNew);
                }
                throw;
            }
        }

        public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, int duration)
        {
            return GetOrAddAsync(key, valueFactory, CachePolicy.Absolute(duration));
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, CachePolicy policy)
        {
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));
            if (policy == null) throw new ArgumentNullException(nameof(policy));

            if (!AppConfig.IsCacheActive)
            {
                return await valueFactory().ConfigureAwait(false);
            }

            var keyNew = ToPhysicalKey(key);
            var newLazy = new Lazy<Task<T>>(valueFactory, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            var cachePolicy = CreateItemPolicy(policy);
            var existing = _cache.AddOrGetExisting(keyNew, newLazy, cachePolicy);
            var added = existing == null;
            var winner = existing as Lazy<Task<T>> ?? newLazy;

            var sw = Stopwatch.StartNew();
            try
            {
                var value = await winner.Value.ConfigureAwait(false);
                sw.Stop();
                if (added)
                {
                    CacheDiagnostics.RecordSet(keyNew, typeof(T), policy);
                    CacheDiagnostics.RecordMiss(keyNew);
                    CacheDiagnostics.RecordLookupDuration(keyNew, false, sw.ElapsedTicks);
                }
                else
                {
                    CacheDiagnostics.RecordHit(keyNew);
                    CacheDiagnostics.RecordLookupDuration(keyNew, true, sw.ElapsedTicks);
                }

                return value;
            }
            catch
            {
                _cache.Remove(keyNew);
                if (added)
                {
                    CacheDiagnostics.RecordMiss(keyNew);
                }
                throw;
            }
        }

        public void Set<T>(string key, T value, int duration)
        {
            Set(key, value, CachePolicy.Absolute(duration));
        }

        public void Set<T>(string key, T value, CachePolicy policy)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));

            if (AppConfig.IsCacheActive)
            {
                var keyNew = ToPhysicalKey(key);
                if (value != null)
                {
                    _cache.Set(keyNew, value, CreateItemPolicy(policy));
                    CacheDiagnostics.RecordSet(keyNew, typeof(T), policy);
                }
            }
        }

        public void Clear(string key)
        {
            // FIX (pre-existing bug): Set/GetOrAdd store under the "Memory:" prefix, so clearing by
            // the raw key removed nothing. Prefix it so targeted eviction works.
            var keyNew = ToPhysicalKey(key);
            _cache.Remove(keyNew, CacheEntryRemovedReason.Removed);
            CacheDiagnostics.RecordRemove(keyNew);
        }

        public int ClearByPrefix(string keyPrefix)
        {
            if (string.IsNullOrEmpty(keyPrefix))
            {
                return 0;
            }

            var physicalPrefix = ToPhysicalKey(keyPrefix);
            var keys = _cache
                .Where(kvp => kvp.Key != null && kvp.Key.StartsWith(physicalPrefix, StringComparison.Ordinal))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keys)
            {
                _cache.Remove(key, CacheEntryRemovedReason.Removed);
                CacheDiagnostics.RecordRemove(key);
            }

            return keys.Count;
        }

        public IEnumerable<KeyValuePair<string, object>> GetAll()
        {
            List<string> cacheKeys = _cache.Select(kvp => kvp.Key).ToList();
            foreach (String key in cacheKeys)
            {
                yield return new KeyValuePair<string, object>(key, _cache[key]);
            }
        }

        public int ClearAll()
        {
            List<string> cacheKeys = _cache.Select(kvp => kvp.Key).ToList();
            foreach (String key in cacheKeys)
            {
                _cache.Remove(key, CacheEntryRemovedReason.Removed);
                CacheDiagnostics.RecordRemove(key);
            }

            var httpRuntimeRemoved = ApplicationCacheClearer.ClearHttpRuntime(_httpRuntimeCacheClearer);
            _logger.LogInformation(
                "MemoryCacheProvider.ClearAll removed {0} data keys (+ {1} HttpRuntime)",
                cacheKeys.Count,
                httpRuntimeRemoved);
            return cacheKeys.Count;
        }

        private static string ToPhysicalKey(string logicalKey)
        {
            if (logicalKey != null && logicalKey.StartsWith(PhysicalKeyPrefix, StringComparison.Ordinal))
            {
                return logicalKey;
            }

            return PhysicalKeyPrefix + logicalKey;
        }

        private static CacheItemPolicy CreateItemPolicy(CachePolicy policy)
        {
            var itemPolicy = new CacheItemPolicy
            {
                Priority = CacheItemPriority.Default,
                RemovedCallback = OnCacheEntryRemoved
            };

            if (policy.Mode == CacheExpirationMode.Sliding)
            {
                itemPolicy.SlidingExpiration = TimeSpan.FromSeconds(policy.DurationSeconds);
            }
            else
            {
                itemPolicy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(policy.DurationSeconds);
            }

            return itemPolicy;
        }

        private static void OnCacheEntryRemoved(CacheEntryRemovedArguments args)
        {
            if (args == null || args.CacheItem == null)
            {
                return;
            }

            var expired = args.RemovedReason == CacheEntryRemovedReason.Expired
                || args.RemovedReason == CacheEntryRemovedReason.Evicted;
            CacheDiagnostics.HandleProviderEviction(args.CacheItem.Key, expired);
        }
    }
}
