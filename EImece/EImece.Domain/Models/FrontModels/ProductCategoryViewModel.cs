using EImece.Domain.GenericRepository;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductCategoryViewModel : ItemListing
    {
        public StorefrontCategoryDto CategoryDto { get; set; }
        public PaginatedList<StorefrontProductCardDto> PagedProductDtos { get; set; }

        public List<StorefrontProductCardDto> CategoryChildrenProducts { get; set; }
        public StorefrontMenuDto ProductMenu { get; set; }
        public StorefrontMenuDto MainPageMenu { get; set; }
        public List<StorefrontCategoryDto> ChildrenProductCategories { get; set; }
        public List<StorefrontBrandDto> StorefrontBrands { get; set; }

        public List<ProductCategoryTreeModel> ProductCategoryTree { get; set; }
        private List<CategoryFilter> _selectedFilterTypes;
        public List<CategoryFilter> SelectedFilterTypes
        {
            get
            {
                if (_selectedFilterTypes == null || !_selectedFilterTypes.Any())
                {
                    _selectedFilterTypes = CreateSelectedFilterList(CategoryFilterTypes);
                }
                return _selectedFilterTypes;
            }
            set
            {
                _selectedFilterTypes = value;
            }
        }
        public SettingValueDto PriceFilterSetting { get; set; }
        public SettingValueDto IsProductPriceEnable { get; set; }
        public SettingValueDto IsProductReviewEnable { get; set; }
        public List<StorefrontProductCardDto> AllProducts { get; set; }

        public ProductCategoryViewModel()
        {
            CategoryChildrenProducts = new List<StorefrontProductCardDto>();
            ChildrenProductCategories = new List<StorefrontCategoryDto>();
            StorefrontBrands = new List<StorefrontBrandDto>();
            AllProducts = new List<StorefrontProductCardDto>();
        }

        public bool IsProductPriceEnabled
        {
            get { return IsProductPriceEnable == null || IsProductPriceEnable.SettingValue.ToBool(true); }
        }

        public bool IsProductReviewEnabled
        {
            get { return IsProductReviewEnable == null || IsProductReviewEnable.SettingValue.ToBool(true); }
        }

        /// <summary>
        /// True when at least one product on this category page has sold quantity &gt; 0.
        /// Used to hide the "En Çok Satan" sort option when it would have no effect.
        /// </summary>
        public bool HasAnySoldProducts
        {
            get { return PagedProductDtos != null && PagedProductDtos.Any(r => r.SoldCount > 0); }
        }

        public bool HasAnyFilters
        {
            get { return CategoryFilterTypes != null && CategoryFilterTypes.Any(r => r.CategoryFilters != null && r.CategoryFilters.Any()); }
        }

        private List<CategoryFilter> CreateSelectedFilterList(List<CategoryFilterType> categoryFilterTypesList)
        {
            if (categoryFilterTypesList == null || !categoryFilterTypesList.Any())
            {
                return new List<CategoryFilter>();
            }
            var selectedFiltersText = Regex.Split(Filter.ToStr(), @"-").Select(r => r.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
            var selectedFilterTypes = new List<CategoryFilter>();
            foreach (var selectedFilter in selectedFiltersText)
            {
                foreach (var categoryFilterType in categoryFilterTypesList)
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
                var categoryFilterHelper = new CategoryFilterHelper(this.PriceFilterSetting);
                if (IsProductPriceEnabled)
                {
                    categoryFilterHelper.AddPriceFilter(categoryFilterTypes);
                }
                if (IsProductReviewEnabled)
                {
                    categoryFilterHelper.AddRatingFilter(categoryFilterTypes);
                }

                if (StorefrontBrands != null && StorefrontBrands.Count >= 1)
                {
                    categoryFilterHelper.AddBrandFilter(categoryFilterTypes, StorefrontBrands);
                }

                SelectedFilterTypes = CreateSelectedFilterList(categoryFilterTypes);
                return categoryFilterTypes;
            }
        }

        public static IDictionary<string, object> GetRouteObjectsForPaging(IPaginatedModelList pagingItems, int page)
        {
            var routeValues = GetRouteValueDictionary(pagingItems);
            routeValues["page"] = page;
            return routeValues;
        }

        public static IDictionary<string, object> GetRouteValueDictionary(IPaginatedModelList pagingItems)
        {
            var routeValues = new Dictionary<string, object>();
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