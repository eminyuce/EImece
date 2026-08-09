using EImece.Domain.Models.Enums;
using System;
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
        public const string ProductArea = "product";
        public const string OrderArea = "order";
        public const string SettingArea = "setting";

        /// <summary>
        /// Prefix shared by every product-list entry (sync + async). Used for bulk invalidation
        /// after admin create/update/delete.
        /// </summary>
        public static string ProductListPrefix
        {
            get { return ProductArea + ":list:"; }
        }

        public static string ProductSearchPrefix
        {
            get { return ProductArea + ":search:"; }
        }

        public static string MainPageProducts(int page, int language)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}mainpage:p{1}:lang{2}",
                ProductListPrefix,
                page,
                language);
        }

        public static string MainPageProductsAsync(int page, int language)
        {
            return MainPageProducts(page, language) + ":async";
        }

        public static string ActiveProducts(int? language)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}active:lang{1}",
                ProductListPrefix,
                language.HasValue ? language.Value.ToString(CultureInfo.InvariantCulture) : "all");
        }

        public static string ActiveProductsAsync(int? language)
        {
            return ActiveProducts(language) + ":async";
        }

        /// <summary>
        /// Search-result key. The search term is normalized (trim + lower invariant) and length-capped
        /// so near-identical queries share an entry and pathological long strings cannot inflate memory.
        /// </summary>
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

        public static string ProductSearchAsync(string search, int page, int pageSize, int language, SortingType sorting)
        {
            return ProductSearch(search, page, pageSize, language, sorting) + ":async";
        }

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

            // Replace characters that would make prefix eviction ambiguous or ugly in diagnostics.
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
    }
}
