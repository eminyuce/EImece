namespace EImece.Domain.Abstractions
{
    /// <summary>
    /// Clears ASP.NET <c>HttpRuntime.Cache</c> (OutputCache profiles, child-action caches, etc.)
    /// without pulling System.Web into the Domain layer.
    /// </summary>
    public interface IHttpRuntimeCacheClearer
    {
        int ClearHttpRuntimeCache();

        /// <summary>
        /// Drops a single ASP.NET OutputCache entry (for example <c>/images/logo.jpg</c>).
        /// No-op when output cache is unused or the path was never cached.
        /// </summary>
        void RemoveOutputCacheItem(string virtualPath);
    }
}
