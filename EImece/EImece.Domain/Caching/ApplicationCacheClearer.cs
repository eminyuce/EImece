using EImece.Domain.Abstractions;
using EImece.Domain.Observability.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using System.Runtime.Caching;

namespace EImece.Domain.Caching
{
    /// <summary>
    /// Shared eviction used by the Admin "Refresh" button and every IEimeceCacheProvider.ClearAll implementation.
    /// </summary>
    public static class ApplicationCacheClearer
    {
        private static ILogger Logger =>
            LoggingBootstrap.LoggerFactory?.CreateLogger(typeof(ApplicationCacheClearer))
            ?? NullLogger.Instance;

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

        public static void ClearAspNetCaches(IHttpRuntimeCacheClearer httpRuntimeCacheClearer, out int httpRuntimeRemoved, out int memoryCacheRemoved)
        {
            httpRuntimeRemoved = httpRuntimeCacheClearer != null
                ? httpRuntimeCacheClearer.ClearHttpRuntimeCache()
                : 0;
            memoryCacheRemoved = ClearMemoryCacheDefault();
            Logger.LogInformation(
                "ApplicationCacheClearer removed {HttpRuntimeRemoved} HttpRuntime + {MemoryCacheRemoved} MemoryCache.Default entries",
                httpRuntimeRemoved,
                memoryCacheRemoved);
        }
    }
}
