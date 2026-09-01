using Microsoft.Extensions.Logging;
using EImece.Domain.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;

namespace EImece.Web.Caching
{
    public sealed class HttpRuntimeCacheClearer : IHttpRuntimeCacheClearer
    {
        private readonly ILogger<HttpRuntimeCacheClearer> _logger;

        public HttpRuntimeCacheClearer(ILogger<HttpRuntimeCacheClearer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public int ClearHttpRuntimeCache()
        {
            var keys = new List<string>();
            try
            {
                var cache = HttpRuntime.Cache;
                if (cache == null)
                {
                    return 0;
                }

                foreach (DictionaryEntry entry in cache)
                {
                    if (entry.Key is string k)
                    {
                        keys.Add(k);
                    }
                }

                foreach (var key in keys)
                {
                    cache.Remove(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ClearHttpRuntimeCache failed after removing {0} keys", keys.Count);
            }

            return keys.Count;
        }

        public void RemoveOutputCacheItem(string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                return;
            }

            try
            {
                var path = virtualPath.Trim();
                if (path.StartsWith("~", StringComparison.Ordinal))
                {
                    path = VirtualPathUtility.ToAbsolute(path);
                }
                else if (!path.StartsWith("/", StringComparison.Ordinal))
                {
                    path = "/" + path;
                }

                HttpResponse.RemoveOutputCacheItem(path);

                var app = HttpRuntime.AppDomainAppVirtualPath;
                if (!string.IsNullOrEmpty(app) &&
                    !string.Equals(app, "/", StringComparison.Ordinal) &&
                    path.IndexOf(app, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    var prefixed = (app.TrimEnd('/') + path);
                    if (!string.Equals(prefixed, path, StringComparison.OrdinalIgnoreCase))
                    {
                        HttpResponse.RemoveOutputCacheItem(prefixed);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RemoveOutputCacheItem failed for {0}", virtualPath);
            }
        }
    }
}
