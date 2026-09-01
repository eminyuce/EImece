using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Helps resolve legacy / modified category slugs (e.g. from IIS 301 redirects,
    /// sitemaps, or old bookmarked URLs) to the current canonical category.
    /// </summary>
    public static class CategorySlugHelper
    {
        public static string NormalizeSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return string.Empty;
            }

            var normalized = slug.Trim().ToLower(CultureInfo.InvariantCulture);
            normalized = normalized.Replace("ı", "i")
                                   .Replace("ğ", "g")
                                   .Replace("ü", "u")
                                   .Replace("ş", "s")
                                   .Replace("ö", "o")
                                   .Replace("ç", "c");

            var sb = new StringBuilder(normalized.Length);
            foreach (var ch in normalized)
            {
                if (char.IsLetterOrDigit(ch) || ch == '-')
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString().Trim('-');
        }

        public static bool SlugMatchesCategoryName(string incomingSlug, string categoryName)
        {
            if (string.IsNullOrWhiteSpace(incomingSlug) || string.IsNullOrWhiteSpace(categoryName))
            {
                return false;
            }

            var incoming = NormalizeSlug(incomingSlug);
            var fromName = NormalizeSlug(GeneralHelper.GetUrlSeoString(categoryName));

            if (incoming.Equals(fromName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(
                NormalizeSlug(incoming.Replace("-", string.Empty)),
                NormalizeSlug(fromName.Replace("-", string.Empty)),
                StringComparison.OrdinalIgnoreCase);
        }

        public static StorefrontCategoryDto FindMatchingCategory(IEnumerable<ProductCategoryTreeModel> tree, string legacySlug)
        {
            if (tree == null || string.IsNullOrWhiteSpace(legacySlug))
            {
                return null;
            }

            foreach (var node in Flatten(tree))
            {
                var category = node != null ? node.ProductCategory : null;
                if (category == null || !category.IsActive)
                {
                    continue;
                }

                var seo = category.GetSeoUrl() ?? string.Empty;
                if (seo.Equals(legacySlug, StringComparison.OrdinalIgnoreCase)
                    || NormalizeSlug(seo).Equals(NormalizeSlug(legacySlug), StringComparison.OrdinalIgnoreCase))
                {
                    return category;
                }

                if (SlugMatchesCategoryName(legacySlug, category.Name))
                {
                    return category;
                }
            }

            return null;
        }

        private static IEnumerable<ProductCategoryTreeModel> Flatten(IEnumerable<ProductCategoryTreeModel> roots)
        {
            foreach (var root in roots.Where(r => r != null))
            {
                yield return root;
                foreach (var child in root.AllChildrens ?? Enumerable.Empty<ProductCategoryTreeModel>())
                {
                    yield return child;
                }
            }
        }
    }
}
