using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EImece.Domain.Caching
{
    public class LazyCacheProvider : IEimeceCacheProvider
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IAppCache _lazyCache = new CachingService();

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
            var keyNew = "Memory:" + key;
            _lazyCache.Remove(keyNew);
            allCacheKeys.TryRemove(keyNew, out _);
        }

        public void ClearAll()
        {
            foreach (var key in allCacheKeys.Keys)
            {
                _lazyCache.Remove(key);
                allCacheKeys.TryRemove(key, out _);
            }
        }

        public T GetOrAdd<T>(string key, Func<T> valueFactory, int duration)
        {
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            // When caching is globally disabled, bypass the cache but still honour the contract.
            if (!AppConfig.IsCacheActive)
            {
                return valueFactory();
            }

            var keyNew = "Memory:" + key;
            // LazyCache wraps the factory in a Lazy<T> internally, guaranteeing single execution
            // under concurrent misses (single-flight) — this is the stampede fix.
            return _lazyCache.GetOrAdd(keyNew, entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(duration);
                allCacheKeys.TryAdd(keyNew, 0);
                return valueFactory();
            });
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, int duration)
        {
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            if (!AppConfig.IsCacheActive)
            {
                return await valueFactory().ConfigureAwait(false);
            }

            var keyNew = "Memory:" + key;
            return await _lazyCache.GetOrAddAsync(keyNew, async entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(duration);
                allCacheKeys.TryAdd(keyNew, 0);
                return await valueFactory().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public bool Get<T>(string key, out T value)
        {
            if (AppConfig.IsCacheActive)
            {
                var keyNew = "Memory:" + key;
                T t = _lazyCache.Get<T>(keyNew);
                if (t == null)
                {
                    value = default(T);
                    return false;
                }
                value = t;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public void Set<T>(string key, T value, int duration)
        {
            if (AppConfig.IsCacheActive)
            {
                var keyNew = "Memory:" + key;
                _lazyCache.Add(keyNew, value, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(duration)
                });
                allCacheKeys.TryAdd(keyNew, 0);
            }
        }
    }
}