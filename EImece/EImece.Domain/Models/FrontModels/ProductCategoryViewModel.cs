using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductCategoryViewModel : ItemListing
    {
        public ProductCategoryDto ProductCategory { get; set; }
        public List<ProductDto> CategoryChildrenProducts { get; set; }
        public MenuDto ProductMenu { get; set; }
        public MenuDto MainPageMenu { get; set; }
        public List<ProductCategoryDto> ChildrenProductCategories { get; set; }
        public List<BrandDto> Brands { get; set; }
        public List<ProductCategoryTreeModel> ProductCategoryTree { get; set; } // Note: This might need to be converted too
        public List<CategoryFilter> SelectedFilterTypes { get; set; }
        public SettingDto PriceFilterSetting { get; set; }
        public List<ProductDto> AllProducts { get; set; }
        
        // Store product IDs associated with this category for filtering logic
        public List<int> ProductIdsInCategory { get; set; }

        public List<ProductDto> Products
        {
            get
            {
                List<ProductDto> result = new List<ProductDto>();
                var products = AllProducts;
                bool hasMinPrice = MinPrice.HasValue && MinPrice.Value > 0;
                bool hasMaxPrice = MaxPrice.HasValue && MaxPrice.Value > 0;
                if (hasMinPrice || hasMaxPrice)
                {
                    if (hasMinPrice && hasMaxPrice)
                    {
                        products = products.Where(r => r.PriceWithDiscount >= MinPrice.Value && r.PriceWithDiscount <= MaxPrice.Value).ToList();
                    }
                    else if (hasMinPrice)
                    {
                        products = products.Where(r => r.PriceWithDiscount >= MinPrice.Value).ToList();
                    }
                    else
                    {
                        products = products.Where(r => r.PriceWithDiscount <= MaxPrice.Value).ToList();
                    }
                }
                
                // Filter products to only those in this category
                if (ProductIdsInCategory != null && ProductIdsInCategory.Any())
                {
                    products = products.Where(p => ProductIdsInCategory.Contains(p.Id)).ToList();
                }
                
                if (!string.IsNullOrEmpty(Filter))
                {
                    // For DTOs, we need to implement filtering differently since CategoryFilterHelper expects entities
                    // For now, we'll skip the advanced filtering for DTOs
                    result = products.ToList();
                }
                else
                {
                    result = products.ToList();
                }

                SelectedFilterTypes = CreateSelectedFilterList();

                switch (Sorting)
                {
                    case Enums.SortingType.Popularity:
                        break;

                    case Enums.SortingType.LowHighPrice:
                        result = result.OrderBy(r => r.Price).ThenByDescending(r => r.Position).ThenByDescending(r => r.UpdatedDate).ToList();
                        break;

                    case Enums.SortingType.HighLowPrice:
                        result = result.OrderByDescending(r => r.Price).ThenByDescending(r => r.Position).ThenByDescending(r => r.UpdatedDate).ToList();
                        break;

                    case Enums.SortingType.AverageRating:
                        break;

                    case Enums.SortingType.AzOrder:
                        break;

                    case Enums.SortingType.ZaOrder:
                        break;

                    default:
                        result = result.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).ToList();
                        break;
                }

                return result;
            }
        }

        private List<CategoryFilter> CreateSelectedFilterList()
        {
            var selectedFiltersText = Regex.Split(Filter.ToStr(), @"-").Select(r => r.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
            var selectedFilterTypes = new List<CategoryFilter>();
            foreach (var selectedFilter in selectedFiltersText)
            {
                foreach (var categoryFilterType in CategoryFilterTypes)
                {
                    selectedFilterTypes.AddRange(categoryFilterType.CategoryFilters.Where(r => r.CategoryFilterId.Equals(selectedFilter, StringComparison.InvariantCultureIgnoreCase)));
                }
            }

            return selectedFilterTypes;
        }

        public string SeoId { get; set; }

        public List<CategoryFilterType> CategoryFilterTypes
        {
            get
            {
                var categoryFilterTypes = new List<CategoryFilterType>();
                // For DTOs, we need to handle this differently since CategoryFilterHelper expects entities
                // For now, we'll skip the brand filtering for DTOs
                var categoryFilterHelper = new CategoryFilterHelper(this.PriceFilterSetting);
                categoryFilterHelper.AddPriceFilter(categoryFilterTypes);
                categoryFilterHelper.AddRatingFilter(categoryFilterTypes);

                // Adjusted logic for DTOs - need to use ProductIdsInCategory to determine which brands have products in this category
                var brandIdsWithProductsInCategory = new HashSet<int>();
                if (ProductIdsInCategory != null && ProductIdsInCategory.Any())
                {
                    // This would need to be populated by the service layer
                    // For now, we'll use the Brands list and filter based on the products in this category
                    var productBrandIds = AllProducts
                        .Where(p => ProductIdsInCategory.Contains(p.Id) && p.BrandId.HasValue)
                        .Select(p => p.BrandId.Value)
                        .Distinct()
                        .ToList();
                    
                    var brandsWithProducts = this.Brands
                        .Where(b => productBrandIds.Contains(b.Id))
                        .OrderBy(b => b.Position)
                        .ThenByDescending(b => b.UpdatedDate)
                        .ToList();
                        
                    // For DTOs, we'll skip the complex brand filtering for now
                }

                return categoryFilterTypes;
            }
        }

        public static RouteValueDictionary GetRouteObjectsForPaging(IPaginatedModelList pagingItems, int page)
        {
            var routeValues = GetRouteValueDictionary(pagingItems);
            routeValues.Add("page", page);
            return routeValues;
        }

        public static RouteValueDictionary GetRouteValueDictionary(IPaginatedModelList pagingItems)
        {
            var routeValues = new RouteValueDictionary();
            if (!string.IsNullOrEmpty(pagingItems.RouteId))
            {
                routeValues.Add("id", pagingItems.RouteId);
            }
            if (pagingItems.Sorting.HasValue && pagingItems.Sorting.Value > 0)
            {
                routeValues.Add("sorting", pagingItems.Sorting);
            }
            if (!string.IsNullOrEmpty(pagingItems.Filter))
            {
                routeValues.Add("filtreler", pagingItems.Filter);
            }
            if (!string.IsNullOrEmpty(pagingItems.Search))
            {
                routeValues.Add("search", pagingItems.Search);
            }
            if (pagingItems.MinPrice.HasValue && pagingItems.MinPrice.Value > 0)
            {
                routeValues.Add("minPrice", pagingItems.MinPrice);
            }
            if (pagingItems.MaxPrice.HasValue && pagingItems.MaxPrice.Value > 0)
            {
                routeValues.Add("maxPrice", pagingItems.MaxPrice);
            }
            return routeValues;
        }
    }
}