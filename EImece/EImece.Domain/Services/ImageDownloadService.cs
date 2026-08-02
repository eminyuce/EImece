using EImece.Domain.Caching;
using EImece.Domain.Observability.Http;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    /// <summary>
    /// Async, resilient image downloader. Constructor-injects <see cref="IResilientHttpClient"/>
    /// (Polly retry/circuit-breaker/timeout) and <see cref="IEimeceCacheProvider"/>. Replaces the
    /// old static <c>ResilientHttpClientAccessor.Instance</c> access and its sync-over-async
    /// <c>.GetAwaiter().GetResult()</c> calls: nothing here blocks an IIS worker thread.
    /// </summary>
    public sealed class ImageDownloadService : IImageDownloadService
    {
        // Historic cap: never buffer more than ~500 KB of image data into memory per download.
        private const int MaxImageBytes = 500000;

        private readonly IResilientHttpClient _httpClient;
        private readonly IEimeceCacheProvider _cache;

        public ImageDownloadService(IResilientHttpClient httpClient, IEimeceCacheProvider cache)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<byte[]> GetImageAsync(string url, Dictionary<string, string> responseHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            // Fully async I/O — the request thread is released for the duration of the download.
            var payload = await _httpClient.GetAsync(url, responseHeaders, cancellationToken).ConfigureAwait(false);
            if (payload?.Content == null || payload.StatusCode != 200)
            {
                return null;
            }

            return payload.Content.Length > MaxImageBytes
                ? payload.Content.Take(MaxImageBytes).ToArray()
                : payload.Content;
        }

        public Task<byte[]> GetImageFromCacheAsync(string url, int minutes = 100, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Single-flight: one download per URL even under concurrent misses. Duration is seconds.
            return _cache.GetOrAddAsync(
                "Image:" + url,
                () => GetImageAsync(url, null, cancellationToken),
                minutes * 60);
        }
    }
}
