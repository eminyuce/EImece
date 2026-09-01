using EImece.Domain.Models.Enums;
using System.Globalization;
using System.Text;

namespace EImece.Domain.Caching
{
    /// <summary>
    /// Canonical cache-key builder. Keys are hierarchical
    /// (<c>{area}:{entity}:{variant}:{dimensions...}</c>) so <see cref="IEimeceCacheProvider.ClearByPrefix"/>
    /// can invalidate whole families without collisions between unrelated features.
    ///
    /// Physical storage may still add a provider prefix (e.g. <c>Memory:</c>); callers pass the
    /// logical key returned here — never the physical one.
    /// </summary>
    public static class CacheKeys
    {
        #region Areas & Prefixes

        public const string ProductArea = "product";
        public const string CategoryArea = "category";
        public const string BrandArea = "brand";
        public const string TagArea = "tag";
        public const string MenuArea = "menu";
        public const string BannerArea = "banner";
        public const string StoryArea = "story";
        public const string FaqArea = "faq";
        public const string OrderArea = "order";
        public const string SettingArea = "setting";
        public const string RssArea = "rss";

        public static string ProductListPrefix => ProductArea + ":list:";
        public static string ProductSearchPrefix => ProductArea + ":search:";
        public static string ProductDetailPrefix => ProductArea + ":detail:";
        public static string ProductRelatedPrefix => ProductArea + ":related:";
        public static string ProductTagPrefix => ProductArea + ":tag:";

        public static string CategoryPrefix => CategoryArea + ":";
        public static string CategoryTreePrefix => CategoryArea + ":tree:";
        public static string CategoryMainPagePrefix => CategoryArea + ":mainpage:";
        public static string CategoryDetailPrefix => CategoryArea + ":detail:";

        public static string BrandPrefix => BrandArea + ":";
        public static string TagPrefix => TagArea + ":";
        public static string MenuPrefix => MenuArea + ":";
        public static string BannerPrefix => BannerArea + ":";
        public static string StoryPrefix => StoryArea + ":";
        public static string FaqPrefix => FaqArea + ":";
        public static string OrderPrefix => OrderArea + ":";
        public static string SettingPrefix => SettingArea + ":";
        public static string RssPrefix => RssArea + ":";
        public static string RssEmailPrefix => RssPrefix + "email:";
        public static string RssFeedPrefix => RssPrefix + "feed:";

        #endregion

        #region Settings

        public static string WebAppManifest => SettingArea + ":webappmanifest";
        /// <summary>JPEG bytes served at <c>/images/logo.jpg</c> by <c>ImagesController.Logo</c>.</summary>
        public static string WebSiteLogoImage => SettingArea + ":logoimage";
        /// <summary>Pre-prefix key used by <c>ImagesController.Logo</c>; still evicted on upload.</summary>
        public const string WebSiteLogoImageLegacy = "WebSiteLogo";
        public static string AllSettings(int language) => string.Format(CultureInfo.InvariantCulture, "{0}all:lang{1}", SettingPrefix, language);
        public static string AllSettingsAsync(int language) => AllSettings(language) + ":async";

        #endregion

        #region Product Keys

        public static string MainPageProducts(int page, int language)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}mainpage:p{1}:lang{2}",
                ProductListPrefix,
                page,
                language);
        }

        public static string MainPageProductsAsync(int page, int language) => MainPageProducts(page, language) + ":async";

        public static string ActiveProducts(int? language)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}active:lang{1}",
                ProductListPrefix,
                language.HasValue ? language.Value.ToString(CultureInfo.InvariantCulture) : "all");
        }

        public static string ActiveProductsAsync(int? language) => ActiveProducts(language) + ":async";

        public static string ProductDetail(int productId)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}id{1}", ProductDetailPrefix, productId);
        }

        public static string ProductDetailAsync(int productId) => ProductDetail(productId) + ":async";

        public static string RelatedProducts(int productId, int categoryId, int language, int count)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}p{1}:cat{2}:lang{3}:c{4}",
                ProductRelatedPrefix,
                productId,
                categoryId,
                language,
                count);
        }

        public static string RelatedProductsAsync(int productId, int categoryId, int language, int count) =>
            RelatedProducts(productId, categoryId, language, count) + ":async";

        public static string ProductsByTag(int tagId, int page, int pageSize, int language, SortingType sorting)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}t{1}:p{2}:ps{3}:lang{4}:sort{5}",
                ProductTagPrefix,
                tagId,
                page,
                pageSize,
                language,
                (int)sorting);
        }

        public static string ProductsByTagAsync(int tagId, int page, int pageSize, int language, SortingType sorting) =>
            ProductsByTag(tagId, page, pageSize, language, sorting) + ":async";

        public static string ProductSearch(string search, int page, int pageSize, int language, SortingType sorting)
        {
            var normalized = NormalizeSearchTerm(search);
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}q{1}:p{2}:ps{3}:lang{4}:sort{5}",
                ProductSearchPrefix,
                normalized,
                page,
                pageSize,
                language,
                (int)sorting);
        }

        public static string ProductSearchAsync(string search, int page, int pageSize, int language, SortingType sorting) =>
            ProductSearch(search, page, pageSize, language, sorting) + ":async";

        #endregion

        #region Category Keys

        public static string CategoryNavigationTree(int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}lang{1}", CategoryTreePrefix, language);
        }

        public static string CategoryNavigationTreeAsync(int language) => CategoryNavigationTree(language) + ":async";

        public static string CategoryMainPage(int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}lang{1}", CategoryMainPagePrefix, language);
        }

        public static string CategoryMainPageAsync(int language) => CategoryMainPage(language) + ":async";

        public static string CategoryDetail(int categoryId)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}id{1}", CategoryDetailPrefix, categoryId);
        }

        public static string CategoryDetailAsync(int categoryId) => CategoryDetail(categoryId) + ":async";

        #endregion

        #region Brand Keys

        public static string BrandList(int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}list:lang{1}", BrandPrefix, language);
        }

        public static string BrandListAsync(int language) => BrandList(language) + ":async";

        public static string BrandDetail(int brandId)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}detail:id{1}", BrandPrefix, brandId);
        }

        public static string BrandDetailAsync(int brandId) => BrandDetail(brandId) + ":async";

        #endregion

        #region Tag Keys

        public static string ProductTags(int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}product:lang{1}", TagPrefix, language);
        }

        public static string ProductTagsAsync(int language) => ProductTags(language) + ":async";

        public static string StoryTags(int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}story:lang{1}", TagPrefix, language);
        }

        public static string StoryTagsAsync(int language) => StoryTags(language) + ":async";

        public static string SimilarProductTags(int productId, int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}similar_product:p{1}:lang{2}", TagPrefix, productId, language);
        }

        public static string SimilarProductTagsAsync(int productId, int language) => SimilarProductTags(productId, language) + ":async";

        public static string SimilarStoryTags(int storyId, int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}similar_story:s{1}:lang{2}", TagPrefix, storyId, language);
        }

        public static string SimilarStoryTagsAsync(int storyId, int language) => SimilarStoryTags(storyId, language) + ":async";

        #endregion

        #region Menu Keys

        public static string MenuTree(int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}tree:lang{1}", MenuPrefix, language);
        }

        public static string MenuTreeAsync(int language) => MenuTree(language) + ":async";

        public static string MenuDetail(int menuId)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}detail:id{1}", MenuPrefix, menuId);
        }

        public static string MenuDetailAsync(int menuId) => MenuDetail(menuId) + ":async";

        #endregion

        #region Banner Keys

        public static string MainPageBanners(int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}mainpage:lang{1}", BannerPrefix, language);
        }

        public static string MainPageBannersAsync(int language) => MainPageBanners(language) + ":async";

        #endregion

        #region Story Keys

        public static string MainPageStories(int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}mainpage:lang{1}", StoryPrefix, language);
        }

        public static string MainPageStoriesAsync(int language) => MainPageStories(language) + ":async";

        public static string StoryDetail(int storyId)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}detail:id{1}", StoryPrefix, storyId);
        }

        public static string StoryDetailAsync(int storyId) => StoryDetail(storyId) + ":async";

        public static string StoryCategories(int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}categories:lang{1}", StoryPrefix, language);
        }

        public static string StoryCategoriesAsync(int language) => StoryCategories(language) + ":async";

        public static string StoriesByCategory(int categoryId, int page, int pageSize, int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}cat{1}:p{2}:ps{3}:lang{4}", StoryPrefix, categoryId, page, pageSize, language);
        }

        public static string StoriesByCategoryAsync(int categoryId, int page, int pageSize, int language) =>
            StoriesByCategory(categoryId, page, pageSize, language) + ":async";

        public static string StoriesByTag(int tagId, int page, int pageSize, int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}tag{1}:p{2}:ps{3}:lang{4}", StoryPrefix, tagId, page, pageSize, language);
        }

        public static string StoriesByTagAsync(int tagId, int page, int pageSize, int language) =>
            StoriesByTag(tagId, page, pageSize, language) + ":async";

        public static string RelatedStories(int storyId, int categoryId, int language, int count)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}related:s{1}:cat{2}:lang{3}:c{4}", StoryPrefix, storyId, categoryId, language, count);
        }

        public static string RelatedStoriesAsync(int storyId, int categoryId, int language, int count) =>
            RelatedStories(storyId, categoryId, language, count) + ":async";

        #endregion

        #region FAQ Keys

        public static string FaqList(int language)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}list:lang{1}", FaqPrefix, language);
        }

        public static string FaqListAsync(int language) => FaqList(language) + ":async";

        #endregion

        #region RSS Keys

        public static string RssEmail(string synKey)
        {
            return RssEmailPrefix + NormalizeSearchTerm(synKey ?? "");
        }

        public static string RssFeed(string url)
        {
            return RssFeedPrefix + NormalizeSearchTerm(url ?? "");
        }

        #endregion

        #region Helpers

        public static string NormalizeSearchTerm(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return "_";
            }

            var trimmed = search.Trim().ToLowerInvariant();
            if (trimmed.Length > 64)
            {
                trimmed = trimmed.Substring(0, 64);
            }

            var sb = new StringBuilder(trimmed.Length);
            foreach (var ch in trimmed)
            {
                if (char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')
                {
                    sb.Append(ch);
                }
                else
                {
                    sb.Append('_');
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}
