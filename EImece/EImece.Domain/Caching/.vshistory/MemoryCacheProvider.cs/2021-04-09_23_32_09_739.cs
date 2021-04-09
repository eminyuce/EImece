using LazyCache;
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
            throw new NotImplementedException();
        }

        public void ClearAll()
        {
            throw new NotImplementedException();
        }

        public bool Get<T1>(string key, out T1 value)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KeyValuePair<string, object>> GetAll()
        {
            throw new NotImplementedException();
        }

        public void Set<T1>(string key, T1 value, int duration)
        {
            throw new NotImplementedException();
        }
    }
}