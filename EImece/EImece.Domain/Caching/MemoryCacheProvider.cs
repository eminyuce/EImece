using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace EImece.Domain.Caching
{
    public class MemoryCacheProvider : IEimeceCacheProvider
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string PhysicalKeyPrefix = "Memory:";
        private MemoryCache _cache = MemoryCache.Default;

        public bool Get<T>(string key, out T value)
        {
            if (AppConfig.IsCacheActive)
            {
                var keyNew = ToPhysicalKey(key);
                var cached = _cache[keyNew];
                if (cached == null)
                {
                    value = default(T);
                    return false;
                }

                // GetOrAdd stores Lazy<T> for single-flight; Set stores T directly.
                if (cached is Lazy<T> lazy)
                {
                    value = lazy.Value;
                    return true;
                }

                if (cached is T typed)
                {
                    value = typed;
                    return true;
                }

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
            var winner = _cache.AddOrGetExisting(keyNew, newLazy, cachePolicy) as Lazy<T> ?? newLazy;

            try
            {
                return winner.Value;
            }
            catch
            {
                // Never cache a faulted factory result.
                _cache.Remove(keyNew);
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
            var winner = _cache.AddOrGetExisting(keyNew, newLazy, cachePolicy) as Lazy<Task<T>> ?? newLazy;

            try
            {
                return await winner.Value.ConfigureAwait(false);
            }
            catch
            {
                _cache.Remove(keyNew);
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
                }
            }
        }

        public void Clear(string key)
        {
            // FIX (pre-existing bug): Set/GetOrAdd store under the "Memory:" prefix, so clearing by
            // the raw key removed nothing. Prefix it so targeted eviction works.
            var keyNew = ToPhysicalKey(key);
            _cache.Remove(keyNew, CacheEntryRemovedReason.Removed);
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
            }

            // HttpRuntime OutputCache + any leftover Default entries (ClearMemoryCacheDefault is
            // largely a no-op here because we already emptied MemoryCache.Default above).
            int httpRuntimeRemoved;
            int memoryCacheRemoved;
            ApplicationCacheClearer.ClearAspNetCaches(out httpRuntimeRemoved, out memoryCacheRemoved);
            Logger.Info(
                "MemoryCacheProvider.ClearAll removed {0} data keys (+ {1} HttpRuntime, {2} MemoryCache.Default)",
                cacheKeys.Count,
                httpRuntimeRemoved,
                memoryCacheRemoved);
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
                Priority = CacheItemPriority.Default
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
    }
}
