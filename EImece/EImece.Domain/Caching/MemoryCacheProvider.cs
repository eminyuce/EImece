using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace EImece.Domain.Caching
{
    public class MemoryCacheProvider : IEimeceCacheProvider
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private MemoryCache _cache = MemoryCache.Default;

        public bool Get<T>(string key, out T value)
        {
            if (AppConfig.IsCacheActive)
            {
                var keyNew = "Memory:" + key;
                if (_cache[keyNew] == null)
                {
                    value = default(T);
                    return false;
                }
                value = (T)_cache[keyNew];
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public T GetOrAdd<T>(string key, Func<T> valueFactory, int duration)
        {
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            if (!AppConfig.IsCacheActive)
            {
                return valueFactory();
            }

            var keyNew = "Memory:" + key;

            // Store a Lazy<T> and publish it with AddOrGetExisting so that concurrent callers
            // resolve the SAME Lazy instance; the value factory therefore executes exactly once
            // (single-flight), preventing the cache stampede of a naive get-then-set.
            var newLazy = new Lazy<T>(valueFactory, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(duration) };
            var winner = _cache.AddOrGetExisting(keyNew, newLazy, policy) as Lazy<T> ?? newLazy;

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

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, int duration)
        {
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            if (!AppConfig.IsCacheActive)
            {
                return await valueFactory().ConfigureAwait(false);
            }

            var keyNew = "Memory:" + key;
            var newLazy = new Lazy<Task<T>>(valueFactory, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            var policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(duration) };
            var winner = _cache.AddOrGetExisting(keyNew, newLazy, policy) as Lazy<Task<T>> ?? newLazy;

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
            if (AppConfig.IsCacheActive)
            {
                var keyNew = "Memory:" + key;
                if (value != null)
                {
                    var policy = new CacheItemPolicy();
                    policy.Priority = CacheItemPriority.Default;
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(duration);
                    _cache.Set(keyNew, value, policy);
                }
            }
        }

        public void Clear(string key)
        {
            // FIX (pre-existing bug): Set/GetOrAdd store under the "Memory:" prefix, so clearing by
            // the raw key removed nothing. Prefix it so targeted eviction works.
            var keyNew = "Memory:" + key;
            _cache.Remove(keyNew, CacheEntryRemovedReason.Removed);
        }

        public IEnumerable<KeyValuePair<string, object>> GetAll()
        {
            List<string> cacheKeys = _cache.Select(kvp => kvp.Key).ToList();
            foreach (String key in cacheKeys)
            {
                yield return new KeyValuePair<string, object>(key, _cache[key]);
            }
        }

        public void ClearAll()
        {
            List<string> cacheKeys = _cache.Select(kvp => kvp.Key).ToList();
            foreach (String key in cacheKeys)
            {
                Clear(key);
            }

            List<string> keys = new List<string>();

            IDictionaryEnumerator enumerator = System.Web.HttpRuntime.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string key = (string)enumerator.Key;
                keys.Add(key);
            }

            foreach (string key in keys)
            {
                System.Web.HttpRuntime.Cache.Remove(key);
            }
        }
    }
}