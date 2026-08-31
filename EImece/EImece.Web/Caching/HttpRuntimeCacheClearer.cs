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
    }
}
