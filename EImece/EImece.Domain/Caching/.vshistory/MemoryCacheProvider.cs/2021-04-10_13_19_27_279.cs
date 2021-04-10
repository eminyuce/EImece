using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace EImece.Domain.Caching
{
    public class MemoryCacheProvider : IEimeceCacheProvider 
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected MemoryCache InitCache()
        {
            return MemoryCache.Default;
        }

        public bool Get<T>(string key, out T value)
        {
            if (AppConfig.IsCacheActive)
            {
                key = "Memory:" + key;
                if (_cache[key] == null)
                {
                    value = default(T);
                    return false;
                }
                value = (T)_cache[key];
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public  void Set<T>(string key, T value, int duration)
        {
            if (AppConfig.IsCacheActive)
            {
                key = "Memory:" + key;
                if (value != null)
                {
                    var policy = new CacheItemPolicy();
                    policy.Priority = CacheItemPriority.Default;
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(duration);
                    _cache.Set(key, value, policy);
                }
            }
        }

        public  void Clear(string key)
        {
            _cache.Remove(key, CacheEntryRemovedReason.Removed);
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
