﻿using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace EImece.Domain.Caching
{
    public class MemoryCacheProvider  : IEimeceCacheProvider
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IAppCache _lazyCache = new CachingService();

        public void Clear(string key)
        {
            _lazyCache.Remove(key);
        }

        public void ClearAll()
        {
            throw new NotImplementedException();
        }

        public bool Get<T>(string key, out T value)
        {
            if (AppConfig.IsCacheActive)
            {
                key = "Memory:" + key;
                if (_lazyCache.Get<T>(key) == null)
                {
                    value = default(T);
                    return false;
                }
                value = (T)_lazyCache[key];
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public IEnumerable<KeyValuePair<string, object>> GetAll()
        {
            throw new NotImplementedException();
        }

        public void Set<T>(string key, T value, int duration)
        {
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();
            options.AbsoluteExpiration =  DateTime.Now.AddSeconds(duration);
            options.SlidingExpiration = TimeSpan.FromSeconds(duration);
            _lazyCache.Add(key, value, options);
        }
    }
}