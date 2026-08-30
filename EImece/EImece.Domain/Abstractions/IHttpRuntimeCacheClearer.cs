namespace EImece.Domain.Abstractions
{
    /// <summary>
    /// Clears ASP.NET <c>HttpRuntime.Cache</c> (OutputCache profiles, child-action caches, etc.)
    /// without pulling System.Web into the Domain layer.
    /// </summary>
    public interface IHttpRuntimeCacheClearer
    {
        int ClearHttpRuntimeCache();
    }
}
