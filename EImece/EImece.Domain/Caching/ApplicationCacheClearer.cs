using EImece.Domain.Abstractions;
using NLog;
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
        public static void ClearAspNetCaches(IHttpRuntimeCacheClearer httpRuntimeCacheClearer, out int httpRuntimeRemoved, out int memoryCacheRemoved)
        {
            httpRuntimeRemoved = httpRuntimeCacheClearer != null
                ? httpRuntimeCacheClearer.ClearHttpRuntimeCache()
                : 0;
            memoryCacheRemoved = ClearMemoryCacheDefault();
            Logger.Info(
                "ApplicationCacheClearer removed {0} HttpRuntime + {1} MemoryCache.Default entries",
                httpRuntimeRemoved,
                memoryCacheRemoved);
        }
    }
}
