using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Resolves legacy category bookmarks such as /c/Ev-Yasam to canonical /c/pc/{seo-hash}/ URLs.
    /// </summary>
    public static class CategorySlugHelper
    {
        private static readonly Regex MultiDash = new Regex("-{2,}", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));

        public static string NormalizeSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return string.Empty;
            }

            var value = slug.Trim().Trim('/').ToLowerInvariant();
            value = MultiDash.Replace(value, "-");
            return value.Trim('-');
        }

        public static bool SlugMatchesCategoryName(string legacySlug, string categoryName)
        {
            if (string.IsNullOrWhiteSpace(legacySlug) || string.IsNullOrWhiteSpace(categoryName))
            {
                return false;
            }

            var incoming = NormalizeSlug(legacySlug);
            var fromName = NormalizeSlug(GeneralHelper.GetUrlSeoString(categoryName));
            if (string.IsNullOrEmpty(incoming) || string.IsNullOrEmpty(fromName))
            {
                return false;
            }

            if (string.Equals(incoming, fromName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Allow /c/Ev-Yasam to match "Ev & Yaşam" → ev--yasam after dash collapse.
            return string.Equals(
                NormalizeSlug(incoming.Replace("-", string.Empty)),
                NormalizeSlug(fromName.Replace("-", string.Empty)),
                StringComparison.OrdinalIgnoreCase);
        }

        public static ProductCategory FindMatchingCategory(IEnumerable<ProductCategoryTreeModel> tree, string legacySlug)
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
