using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductCategoryViewModel : ItemListing
    {
        public ProductCategory ProductCategory { get; set; }
        public List<Product> CategoryChildrenProducts { get; set; }
        public Menu ProductMenu { get; set; }
        public Menu MainPageMenu { get; set; }
        public List<ProductCategory> ChildrenProductCategories { get; set; }
        public List<Brand> Brands { get; set; }
        public List<ProductCategoryTreeModel> ProductCategoryTree { get; set; }
        public List<CategoryFilter> SelectedFilterTypes { get; set; }

        public List<Product> AllProducts { get; set; }

        public List<Product> Products
        {
            get
            {
                List<Product> result = new List<Product>();
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
                if (!string.IsNullOrEmpty(Filter))
                {
                    var categoryFilterHelper = new CategoryFilterHelper(CategoryFilterTypes, SelectedFilters);
                    ICollection<Product> filteredProducts = categoryFilterHelper.FilterProductsByPrice(products);
                    filteredProducts = categoryFilterHelper.FilterProductsByRating(filteredProducts);
                    filteredProducts = categoryFilterHelper.FilterProductsByBrand(filteredProducts);
                    result = filteredProducts.ToList();
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
                var categoryFilterHelper = new CategoryFilterHelper();
                categoryFilterHelper.AddPriceFilter(categoryFilterTypes);
                categoryFilterHelper.AddRatingFilter(categoryFilterTypes);

                var brandsWithProducts =
                from t1 in ProductCategory.Products.ToList()
                join t2 in this.Brands on t1.BrandId equals t2.Id
                orderby t2.Position, t2.UpdatedDate
                select t2;
                categoryFilterHelper.AddBrandFilter(categoryFilterTypes, brandsWithProducts.Distinct().ToList());
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