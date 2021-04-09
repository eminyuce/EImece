using LazyCache;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace EImece.Domain.Caching
{
    public class MemoryCacheProvider<T> : IEimeceCacheProvider
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IAppCache _lazyCache = new CachingService();
    

        public bool Get<T>(string key, out T value)
        {
            if (AppConfig.IsCacheActive)
            {
                key = "Memory:" + key;
                value = default;
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
                key = "Memory:" + key;
                if (value != null)
                {
                    
                }
            }
        }

        public void Clear(string key)
        {
            _lazyCache.Remove(key);
        }

        public   IEnumerable<KeyValuePair<string, object>> GetAll()
        {
             
        }

        public   void ClearAll()
        {
          
        }
    }
}