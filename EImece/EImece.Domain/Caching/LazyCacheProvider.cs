using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using System;
using System.Collections.Concurrent;

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
            _lazyCache.Remove(key);
        }

        public void ClearAll()
        {
            foreach (var key in allCacheKeys.Keys)
            {
                _lazyCache.Remove(key);
                allCacheKeys.TryRemove(key, out _);
            }
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