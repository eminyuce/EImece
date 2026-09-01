using EImece.Domain.Abstractions;
using EImece.Domain.Services.IServices;
using System;

namespace EImece.Domain.Caching
{
    /// <summary>
    /// Shared admin cache-maintenance operations used by Cache Admin (and the legacy
    /// Dashboard ClearCache alias). Keeps a single implementation of each invalidation path.
    /// </summary>
    public static class AdminCacheMaintenance
    {
        public static int ClearAllData(
            ISettingService settingService,
            IProductService productService,
            IEimeceCacheProvider cache)
        {
            if (settingService == null) throw new ArgumentNullException(nameof(settingService));
            if (productService == null) throw new ArgumentNullException(nameof(productService));
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            settingService.ClearCache();
            productService.InvalidateProductListCaches();
            return cache.ClearAll();
        }

        /// <summary>
        /// Targeted or full storefront invalidation. Returns removed provider keys for prefix
        /// purges, or the <see cref="IEimeceCacheProvider.ClearAll"/> count for <c>all</c>.
        /// Unknown <paramref name="target"/> returns -1.
        /// </summary>
        public static int Invalidate(
            string target,
            ISettingService settingService,
            IProductService productService,
            IProductCategoryService productCategoryService,
            IEimeceCacheProvider cache,
            IHttpRuntimeCacheClearer httpRuntimeCacheClearer,
            out bool fullWipe)
        {
            fullWipe = false;
            if (string.IsNullOrWhiteSpace(target) ||
                settingService == null ||
                productService == null ||
                productCategoryService == null ||
                cache == null)
            {
                return -1;
            }

            var removed = 0;
            switch (target.Trim().ToLowerInvariant())
            {
                case "products":
                    productService.InvalidateProductListCaches();
                    break;

                case "categories":
                    productCategoryService.InvalidateCategoryCaches();
                    break;

                case "settings":
                    settingService.ClearCache();
                    break;

                case "content":
                    removed += cache.ClearByPrefix(CacheKeys.StoryPrefix);
                    removed += cache.ClearByPrefix(CacheKeys.MenuPrefix);
                    removed += cache.ClearByPrefix(CacheKeys.BannerPrefix);
                    removed += cache.ClearByPrefix(CacheKeys.FaqPrefix);
                    removed += cache.ClearByPrefix(CacheKeys.TagPrefix);
                    removed += cache.ClearByPrefix(CacheKeys.BrandPrefix);
                    removed += cache.ClearByPrefix(CacheKeys.RssPrefix);
                    break;

                case "all":
                    removed = ClearAllData(settingService, productService, cache);
                    fullWipe = true;
                    break;

                default:
                    return -1;
            }

            if (!fullWipe)
            {
                ApplicationCacheClearer.ClearHttpRuntime(httpRuntimeCacheClearer);
            }

            return removed;
        }

        /// <summary>
        /// After an admin logo upload: drop setting rows, the <c>/images/logo.jpg</c> byte cache,
        /// and the OutputCache entry for that URL. Does not wipe the rest of the storefront cache.
        /// </summary>
        public static void InvalidateWebsiteLogo(
            ISettingService settingService,
            IEimeceCacheProvider cache,
            IHttpRuntimeCacheClearer httpRuntimeCacheClearer)
        {
            if (settingService == null) throw new ArgumentNullException(nameof(settingService));

            settingService.ClearCache();
            if (cache != null)
            {
                cache.Clear(CacheKeys.WebSiteLogoImage);
                cache.Clear(CacheKeys.WebSiteLogoImageLegacy);
            }

            if (httpRuntimeCacheClearer != null)
            {
                httpRuntimeCacheClearer.RemoveOutputCacheItem(Constants.LogoImagePath);
            }
        }
    }
}
