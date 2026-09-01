using EImece.Domain.Abstractions;
using EImece.Domain.Observability.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EImece.Domain.Caching
{
    /// <summary>
    /// Shared OutputCache eviction used by the Admin Refresh button and every
    /// <see cref="IEimeceCacheProvider.ClearAll"/> implementation.
    /// Application data must go through <see cref="IEimeceCacheProvider"/> — not a second cache host.
    /// </summary>
    public static class ApplicationCacheClearer
    {
        private static ILogger Logger =>
            LoggingBootstrap.LoggerFactory?.CreateLogger(typeof(ApplicationCacheClearer))
            ?? NullLogger.Instance;

        public static int ClearHttpRuntime(IHttpRuntimeCacheClearer httpRuntimeCacheClearer)
        {
            var removed = httpRuntimeCacheClearer != null
                ? httpRuntimeCacheClearer.ClearHttpRuntimeCache()
                : 0;
            Logger.LogInformation(
                "ApplicationCacheClearer removed {HttpRuntimeRemoved} HttpRuntime entries",
                removed);
            return removed;
        }
    }
}
