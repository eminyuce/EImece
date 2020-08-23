using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Helpers
{
    public static class EntityFilterHelper
    {
        public static ProductCategory FilterProductCategory(ProductCategory productCategory)
        {
            if (productCategory == null)
            {
                return productCategory;
            }
            productCategory.Products = FilterProducts(productCategory.Products);

            return productCategory;
        }

        public static ICollection<Product> FilterProducts(ICollection<Product> items)
        {
            var result = new List<Product>();
            if (items == null)
            {
                return result;
            }
            foreach (var item in items)
            {
                FilterProduct(item);
            }

            return items.Where(r => r.IsActive).OrderBy(r => r.Position).ToList();
        }

        public static void FilterProduct(Product item)
        {
            item.ProductTags = FilterProductTags(item.ProductTags);
            item.ProductSpecifications = FilterProductSpecifications(item.ProductSpecifications);
        }

        public static ICollection<ProductSpecification> FilterProductSpecifications(ICollection<ProductSpecification> items)
        {
            var result = new List<ProductSpecification>();
            if (items == null)
            {
                return result;
            }
            return items.Where(r => r.IsActive).OrderBy(r => r.Position).ToList();
        }

        public static ICollection<ProductTag> FilterProductTags(ICollection<ProductTag> items)
        {
            var result = new List<ProductTag>();
            if (items == null)
            {
                return result;
            }
            foreach (var item in items)
            {
                if (item.Tag != null && item.Tag.IsActive)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public static List<TagCategory> FilterTagCategories(List<TagCategory> tagCategories)
        {
            var result = new List<TagCategory>();
            if (tagCategories.IsEmpty())
            {
                return result;
            }
            foreach (var item in tagCategories)
            {
                result.Add(FilterTagCategory(item));
            }
            return tagCategories;
        }

        public static TagCategory FilterTagCategory(TagCategory tagCategory)
        {
            if (tagCategory == null)
            {
                return tagCategory;
            }
            tagCategory.Tags = FilterTags(tagCategory.Tags);
            return tagCategory;
        }

        public static ICollection<Tag> FilterTags(ICollection<Tag> items)
        {
            if (items == null)
            {
                return items;
            }
            return items.Where(r => r.IsActive).OrderBy(r => r.Position).ToList();
        }

        public static ICollection<ProductCategory> FilterProductCategories(ICollection<ProductCategory> items)
        {
            if (items == null)
            {
                return items;
            }
            return items.Where(r => r.IsActive).OrderBy(r => r.Position).ToList();
        }

        public static ICollection<ProductComment> FilterProductComments(ICollection<ProductComment> items)
        {
            if (items == null)
            {
                return items;
            }
            return items.Where(r => r.IsActive).OrderBy(r => r.Position).ToList();
        }
    }
}