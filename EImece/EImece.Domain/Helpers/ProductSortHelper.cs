using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Shared storefront product ordering:
    /// Position (order), then MainPage (vitrine), then IsCampaign (campaign), then UpdatedDate.
    /// </summary>
    public static class ProductSortHelper
    {
        public static IOrderedQueryable<Product> OrderByStorefrontDefault(this IQueryable<Product> products)
        {
            return products
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.MainPage)
                .ThenByDescending(r => r.IsCampaign)
                .ThenByDescending(r => r.UpdatedDate);
        }

        public static IOrderedEnumerable<Product> OrderByStorefrontDefault(this IEnumerable<Product> products)
        {
            return products
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.MainPage)
                .ThenByDescending(r => r.IsCampaign)
                .ThenByDescending(r => r.UpdatedDate);
        }

        public static IOrderedQueryable<StorefrontProductCardDto> OrderByStorefrontDefault(this IQueryable<StorefrontProductCardDto> products)
        {
            return products
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.MainPage)
                .ThenByDescending(r => r.IsCampaign)
                .ThenByDescending(r => r.UpdatedDate);
        }

        public static IOrderedEnumerable<StorefrontProductCardDto> OrderByStorefrontDefault(this IEnumerable<StorefrontProductCardDto> products)
        {
            return products
                .OrderBy(r => r.Position)
                .ThenByDescending(r => r.MainPage)
                .ThenByDescending(r => r.IsCampaign)
                .ThenByDescending(r => r.UpdatedDate);
        }

        public static IOrderedQueryable<ProductTag> OrderByProductStorefrontDefault(this IQueryable<ProductTag> productTags)
        {
            return productTags
                .OrderBy(r => r.Product.Position)
                .ThenByDescending(r => r.Product.MainPage)
                .ThenByDescending(r => r.Product.IsCampaign)
                .ThenByDescending(r => r.Product.UpdatedDate);
        }

        public static IOrderedEnumerable<Product> ThenByStorefrontDefault(this IOrderedEnumerable<Product> products)
        {
            return products
                .ThenBy(r => r.Position)
                .ThenByDescending(r => r.MainPage)
                .ThenByDescending(r => r.IsCampaign)
                .ThenByDescending(r => r.UpdatedDate);
        }

        public static IOrderedEnumerable<StorefrontProductCardDto> ThenByStorefrontDefault(this IOrderedEnumerable<StorefrontProductCardDto> products)
        {
            return products
                .ThenBy(r => r.Position)
                .ThenByDescending(r => r.MainPage)
                .ThenByDescending(r => r.IsCampaign)
                .ThenByDescending(r => r.UpdatedDate);
        }
    }
}
