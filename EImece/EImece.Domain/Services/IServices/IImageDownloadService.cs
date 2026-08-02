using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    /// <summary>
    /// Downloads images/binary content over HTTP using the resilient (Polly-backed) client.
    /// This is the dependency-injected replacement for the former static
    /// <c>ResilientHttpClientAccessor</c> service-locator: infrastructure (the HTTP client) is now
    /// supplied via the constructor instead of reached through global static state.
    /// </summary>
    public interface IImageDownloadService
    {
        /// <summary>
        /// Downloads the resource at <paramref name="url"/> asynchronously. Optionally fills
        /// <paramref name="responseHeaders"/> with the response headers. Returns null on failure.
        /// </summary>
        Task<byte[]> GetImageAsync(string url, Dictionary<string, string> responseHeaders = null, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Cached, single-flight variant of <see cref="GetImageAsync"/>. Concurrent requests for the
        /// same URL share one download and one cache entry (no stampede).
        /// </summary>
        Task<byte[]> GetImageFromCacheAsync(string url, int minutes = 100, CancellationToken cancellationToken = default(CancellationToken));
    }
}
