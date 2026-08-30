using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace EImece.Domain.Caching
{
    /// <summary>
    /// Shared eviction used by the Admin "Refresh" button (<c>DashboardController.ClearCache</c>)
    /// and every <see cref="IEimeceCacheProvider.ClearAll"/> implementation.
    ///
    /// A single-process ASP.NET MVC 5 host keeps data in LazyCache / <see cref="MemoryCache.Default"/>
    /// and rendered HTML in <c>HttpRuntime.Cache</c> (OutputCache / partial-view cache). Clearing
    /// only the data layer leaves storefront pages serving stale HTML until the profile TTL expires.
    /// </summary>
    public static class ApplicationCacheClearer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Clears ASP.NET <c>HttpRuntime.Cache</c> (OutputCache profiles, child-action caches, etc.).
        /// </summary>
        public static int ClearHttpRuntimeCache()
        {
            var keys = new List<string>();
            try
            {
                var httpRuntimeType = Type.GetType("System.Web.HttpRuntime, System.Web");
                if (httpRuntimeType == null) return 0;

                var cacheProp = httpRuntimeType.GetProperty("Cache");
                var cache = cacheProp?.GetValue(null) as IEnumerable;
                if (cache == null) return 0;

                var removeMethod = cache.GetType().GetMethod("Remove", new[] { typeof(string) });
                foreach (DictionaryEntry entry in cache)
                {
                    if (entry.Key is string k)
                    {
                        keys.Add(k);
                    }
                }

                foreach (var key in keys)
                {
                    removeMethod?.Invoke(cache, new object[] { key });
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "ClearHttpRuntimeCache failed after removing {0} keys", keys.Count);
            }

            return keys.Count;
        }

        /// <summary>
        /// Clears <see cref="MemoryCache.Default"/> (RssHelper and any direct Runtime.Caching writers).
        /// Does not touch LazyCache's private MemoryCache — that is owned by <see cref="LazyCacheProvider"/>.
        /// </summary>
        public static int ClearMemoryCacheDefault()
        {
            var cache = MemoryCache.Default;
            var keys = cache.Select(kvp => kvp.Key).ToList();
            foreach (var key in keys)
            {
                cache.Remove(key);
            }

            return keys.Count;
        }

        /// <summary>
        /// Full process-wide wipe used by Admin Refresh: HttpRuntime + MemoryCache.Default.
        /// Callers must also clear their own provider store (LazyCache keys / MemoryCacheProvider entries).
        /// </summary>
        public static void ClearAspNetCaches(out int httpRuntimeRemoved, out int memoryCacheRemoved)
        {
            httpRuntimeRemoved = ClearHttpRuntimeCache();
            memoryCacheRemoved = ClearMemoryCacheDefault();
            Logger.Info(
                "ApplicationCacheClearer removed {0} HttpRuntime + {1} MemoryCache.Default entries",
                httpRuntimeRemoved,
                memoryCacheRemoved);
        }
    }
}
