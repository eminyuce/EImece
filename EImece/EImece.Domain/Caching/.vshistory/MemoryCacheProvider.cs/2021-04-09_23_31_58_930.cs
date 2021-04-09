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
     
 
    }
}