using Microsoft.Extensions.Logging;
using EImece.Domain.Abstractions;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace EImece.Domain.Caching
{
    public class LazyCacheProvider : IEimeceCacheProvider
    {
        private readonly ILogger<LazyCacheProvider> _logger;

        private const string PhysicalKeyPrefix = "Memory:";
        private readonly IAppCache _lazyCache;
        private readonly IHttpRuntimeCacheClearer _httpRuntimeCacheClearer;

        public LazyCacheProvider(
            ILogger<LazyCacheProvider> logger,
            IMemoryCache memoryCache,
            IHttpRuntimeCacheClearer httpRuntimeCacheClearer = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (memoryCache == null) throw new ArgumentNullException(nameof(memoryCache));
            _lazyCache = new CachingService(new Lazy<ICacheProvider>(() => new LazyCache.Providers.MemoryCacheProvider(memoryCache)));
            _httpRuntimeCacheClearer = httpRuntimeCacheClearer;
        }

        // ConcurrentDictionary replaces a HashSet that was only locked on writes: ClearAll()
        // used to enumerate it without the lock, which races with concurrent Set() calls.
        // The dictionary's snapshot enumerator is safe to iterate while other threads mutate it.
        private static readonly ConcurrentDictionary<string, byte> allCacheKeys =
            new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);

        public void Clear(string key)
        {
            // FIX (pre-existing bug): entries are stored under the "Memory:" prefix by Set/GetOrAdd,
            // but Clear used the raw key, so targeted eviction never actually removed anything
            // (e.g. SettingService clearing its cache after a save was a no-op). Prefix it to match.
            var keyNew = ToPhysicalKey(key);
            _lazyCache.Remove(keyNew);
            allCacheKeys.TryRemove(keyNew, out _);
        }

        public int ClearByPrefix(string keyPrefix)
        {
            if (string.IsNullOrEmpty(keyPrefix))
            {
                return 0;
            }

            var physicalPrefix = ToPhysicalKey(keyPrefix);
            var keys = allCacheKeys.Keys
                .Where(k => k.StartsWith(physicalPrefix, StringComparison.Ordinal))
                .ToList();

            foreach (var key in keys)
            {
                _lazyCache.Remove(key);
                allCacheKeys.TryRemove(key, out _);
            }

            return keys.Count;
        }

        public int ClearAll()
        {
            var keys = allCacheKeys.Keys.ToList();
            foreach (var key in keys)
            {
                _lazyCache.Remove(key);
                allCacheKeys.TryRemove(key, out _);
            }

            // Admin Refresh must also drop OutputCache HTML and MemoryCache.Default — otherwise
            // ProductsController/[CustomOutputCache] keeps serving stale pages after data eviction.
            int httpRuntimeRemoved;
            int memoryCacheRemoved;
            ApplicationCacheClearer.ClearAspNetCaches(_httpRuntimeCacheClearer, out httpRuntimeRemoved, out memoryCacheRemoved);
            _logger.LogInformation(
                "LazyCacheProvider.ClearAll removed {0} data keys (+ {1} HttpRuntime, {2} MemoryCache.Default)",
                keys.Count,
                httpRuntimeRemoved,
                memoryCacheRemoved);
            return keys.Count;
        }

        public T GetOrAdd<T>(string key, Func<T> valueFactory, int duration)
        {
            return GetOrAdd(key, valueFactory, CachePolicy.Absolute(duration));
        }

        public T GetOrAdd<T>(string key, Func<T> valueFactory, CachePolicy policy)
        {
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));
            if (policy == null) throw new ArgumentNullException(nameof(policy));

            // When caching is globally disabled, bypass the cache but still honour the contract.
            if (!AppConfig.IsCacheActive)
            {
                return valueFactory();
            }

            var keyNew = ToPhysicalKey(key);
            // LazyCache wraps the factory in a Lazy<T> internally, guaranteeing single execution
            // under concurrent misses (single-flight) — this is the stampede fix.
            return _lazyCache.GetOrAdd(keyNew, entry =>
            {
                ApplyPolicy(entry, policy);
                allCacheKeys.TryAdd(keyNew, 0);
                return valueFactory();
            });
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
            return await _lazyCache.GetOrAddAsync(keyNew, async entry =>
            {
                ApplyPolicy(entry, policy);
                allCacheKeys.TryAdd(keyNew, 0);
                return await valueFactory().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public bool Get<T>(string key, out T value)
        {
            if (AppConfig.IsCacheActive)
            {
                var keyNew = ToPhysicalKey(key);
                if (_lazyCache.CacheProvider.TryGetValue<object>(keyNew, out var raw) && raw != null)
                {
                    if (raw is T typedValue)
                    {
                        value = typedValue;
                        return true;
                    }
                }
            }
            value = default(T);
            return false;
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
                var options = new MemoryCacheEntryOptions();
                ApplyPolicy(options, policy);
                _lazyCache.Add(keyNew, value, options);
                allCacheKeys.TryAdd(keyNew, 0);
            }
        }

        private static string ToPhysicalKey(string logicalKey)
        {
            if (logicalKey != null && logicalKey.StartsWith(PhysicalKeyPrefix, StringComparison.Ordinal))
            {
                return logicalKey;
            }

            return PhysicalKeyPrefix + logicalKey;
        }

        private static void ApplyPolicy(ICacheEntry entry, CachePolicy policy)
        {
            MemoryCacheEntrySizing.Apply(entry);

            if (policy.Mode == CacheExpirationMode.Sliding)
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(policy.DurationSeconds);
            }
            else
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(policy.DurationSeconds);
            }
        }

        private static void ApplyPolicy(MemoryCacheEntryOptions options, CachePolicy policy)
        {
            MemoryCacheEntrySizing.Apply(options);

            if (policy.Mode == CacheExpirationMode.Sliding)
            {
                options.SlidingExpiration = TimeSpan.FromSeconds(policy.DurationSeconds);
            }
            else
            {
                options.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(policy.DurationSeconds);
            }
        }
    }
}
