using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Caching
{
    public class MemoryCacheProvider : CacheProvider<MemoryCache>
    {
        protected override MemoryCache InitCache()
        {
            return MemoryCache.Default;
        }

        public override bool Get<T>(string key, out T value)
        {
            try
            {
                if (_cache[key] == null)
                {
                    value = default(T);
                    return false;
                }

                value = (T)_cache[key];
            }
            catch
            {
                value = default(T);
                return false;
            }

            return true;
        }

        public override void Set<T>(string key, T value)
        {
            Set<T>(key, value, CacheDuration);
        }

        public override void Set<T>(string key, T value, int duration)
        {
            CacheItemPolicy policy = null;
            policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.Default;
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(duration);
            _cache.Set(key, value, policy);
        }

        public override void Clear(string key)
        {
            _cache.Remove(key);
        }

        public override IEnumerable<KeyValuePair<string, object>> GetAll()
        {
            List<string> cacheKeys = _cache.Select(kvp => kvp.Key).ToList();
            foreach (String key in cacheKeys)
            {
                yield return new KeyValuePair<string, object>(key as string, _cache[key]);
            }

        }

        public override void ClearAll()
        {
            List<string> cacheKeys = _cache.Select(kvp => kvp.Key).ToList();
            foreach (String key in cacheKeys)
            {
                Clear(key);
            }
        }
    }
}
