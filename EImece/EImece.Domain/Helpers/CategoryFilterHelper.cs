using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace EImece.Domain.Helpers
{
    public class CategoryFilterHelper
    {
        private List<CategoryFilterType> categoryFilterTypes;
        private List<string> selectedFilters;
        private Setting priceFilterSetting;

        public CategoryFilterHelper(Setting priceFilterSetting)
        {
            this.priceFilterSetting = priceFilterSetting;
        }

        public CategoryFilterHelper(List<CategoryFilterType> categoryFilterTypes, List<string> selectedFilters)
        {
            this.categoryFilterTypes = categoryFilterTypes;
            this.selectedFilters = selectedFilters;
        }

        public ICollection<Product> FilterProductsByPrice(ICollection<Product> products)
        {
            bool hasPriceFilter = false;
            List<Product> filteredProducts = new List<Product>();
            foreach (var categoryFilter in categoryFilterTypes)
            {
                foreach (var filterId in selectedFilters)
                {
                    if (categoryFilter.CategoryFilters.Any(t => t.
                    CategoryFilterId.Equals(filterId, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var filterProperty = categoryFilter.CategoryFilters.
                            FirstOrDefault(t => t.CategoryFilterId == filterId);
                        switch (categoryFilter.FilterTypeName.FilterType)
                        {
                            case FilterType.Price:
                                filteredProducts.AddRange(products.Where(r => r.PriceWithDiscount >= filterProperty.minPrice && r.PriceWithDiscount < filterProperty.maxPrice).ToList());
                                hasPriceFilter = true;
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            return hasPriceFilter ? filteredProducts : products;
        }

        public ICollection<Product> FilterProductsByRating(ICollection<Product> products)
        {
            bool hasFilter = false;
            List<Product> filteredProducts = new List<Product>();
            foreach (var categoryFilter in categoryFilterTypes)
            {
                foreach (var filterId in selectedFilters)
                {
                    if (categoryFilter.CategoryFilters.Any(t =>
                    t.CategoryFilterId.Equals(filterId,
                    StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var filterProperty = categoryFilter.CategoryFilters.
                            FirstOrDefault(t => t.CategoryFilterId == filterId);
                        switch (categoryFilter.FilterTypeName.FilterType)
                        {
                            case FilterType.Rating:
                                filteredProducts.AddRange(products.Where(r => r.Rating >= filterProperty.ItemId && r.Rating < filterProperty.ItemId + 1).ToList());
                                hasFilter = true;
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            return hasFilter ? filteredProducts : products;
        }

        public ICollection<Product> FilterProductsByBrand(ICollection<Product> products)
        {
            bool hasFilter = false;
            List<Product> filteredProducts = new List<Product>();
            foreach (var categoryFilter in categoryFilterTypes)
            {
                foreach (var filterId in selectedFilters)
                {
                    if (categoryFilter.CategoryFilters.Any(t => t.
                    CategoryFilterId.Equals(filterId,
                    StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var filterProperty = categoryFilter.CategoryFilters.FirstOrDefault(t => t.CategoryFilterId == filterId);
                        switch (categoryFilter.FilterTypeName.FilterType)
                        {
                            case FilterType.Brand:
                                filteredProducts.AddRange(products.Where(r => r.BrandId >= filterProperty.ItemId).ToList());
                                hasFilter = true;
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            return hasFilter ? filteredProducts : products;
        }

        public void AddBrandFilter(List<CategoryFilterType> categoryFilterTypes, List<Brand> brands)
        {
            if (brands.IsEmpty()) return;

            var item = new CategoryFilterType
            {
                Position = 0,
                FilterTypeName = new FilterTypeName
                {
                    FilterType = FilterType.Brand,
                    Text = Resource.Brands
                }
            };

            item.CategoryFilters = brands
                .Select(brand => new CategoryFilter
                {
                    CategoryFilterId = $"b{brand.Id}",
                    name = brand.Name
                })
                .ToList();

            item.CategoryFilters.ForEach(f => f.Parent = item);
            categoryFilterTypes.Add(item);
        }


        public void AddPriceFilter(List<CategoryFilterType> categoryFilterTypes)
        {
            PriceFilterConfig priceRanges = ReadPriceFilterFromSetting();

            var item = new CategoryFilterType
            {
                Position = 1,
                FilterTypeName = new FilterTypeName { FilterType = FilterType.Price, Text = Resource.Price }
            };

            for (int i = 0; i < priceRanges.PriceRanges.Count; i++)
            {
                PriceRange priceRange = priceRanges.PriceRanges[i];
                var filter = new CategoryFilter
                {
                    CategoryFilterId = $"p{100 + i}",
                    minPrice = priceRange.Min,
                    maxPrice = priceRange.Max
                };

                filter.name = priceRange.IsLast
                    ? $"{priceRange.Min.CurrencySign()} {Resource.AndOverPrice}"
                    : i == 0
                        ? $"{priceRange.Max.CurrencySign()} {Resource.AndUnderPrice}"
                        : $"{priceRange.Min.CurrencySign()} - {priceRange.Max.CurrencySign()}";

                item.CategoryFilters.Add(filter);
            }

            item.CategoryFilters.ForEach(f => f.Parent = item);
            categoryFilterTypes.Add(item);
        }
        //       "PriceFilterConfig": {
        // "PriceRanges": [
        //   { "Min": 0, "Max": 49, "IsLast": false },
        //   { "Min": 49, "Max": 99, "IsLast": false },
        //   { "Min": 99, "Max": 499, "IsLast": false },
        //   { "Min": 499, "Max": 999, "IsLast": false },
        //   { "Min": 999, "Max": 4999, "IsLast": false },
        //   { "Min": 4999, "Max": 9999999, "IsLast": true }
        // ]
        //}

        private PriceFilterConfig ReadPriceFilterFromSetting()
        {
            if (priceFilterSetting.IsEmpty())
            {
                // Return default hardcoded price ranges
                return new PriceFilterConfig
                {
                    PriceRanges = new List<PriceRange>
                        {
                            new PriceRange { Min = 0, Max = 49, IsLast = false },
                            new PriceRange { Min = 49, Max = 99, IsLast = false },
                            new PriceRange { Min = 99, Max = 499, IsLast = false },
                            new PriceRange { Min = 499, Max = 999, IsLast = false },
                            new PriceRange { Min = 999, Max = 4999, IsLast = false },
                            new PriceRange { Min = 4999, Max = 9999999, IsLast = true }
                                }
                    };
            }
            else
            {
                var json = priceFilterSetting.SettingValue.ToStr();
                var result =  JsonConvert.DeserializeObject<PriceFilterConfig>(json);
                return result;
            }
        }

        public void AddRatingFilter(List<CategoryFilterType> categoryFilterTypes)
        {
            var item = new CategoryFilterType
            {
                Position = 5,
                FilterTypeName = new FilterTypeName
                {
                    FilterType = FilterType.Rating,
                    Text = Resource.Rating
                }
            };

            for (int rating = 5; rating >= 1; rating--)
            {
                var filter = new CategoryFilter
                {
                    CategoryFilterId = $"r{rating}",
                    name = $"{rating} {Resource.Star}",
                    rating = rating
                };
                item.CategoryFilters.Add(filter);
            }

            item.CategoryFilters.ForEach(f => f.Parent = item);
            categoryFilterTypes.Add(item);
        }
    }
    public class PriceRange
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public bool IsLast { get; set; }
    }

    public class PriceFilterConfig
    {
        public List<PriceRange> PriceRanges { get; set; }
    }
}