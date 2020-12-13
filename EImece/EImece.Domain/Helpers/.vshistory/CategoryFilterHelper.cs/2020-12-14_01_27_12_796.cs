using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Helpers
{
    public class CategoryFilterHelper
    {
        private List<CategoryFilterType> categoryFilterTypes;
        private List<string> selectedFilters;

        public CategoryFilterHelper()
        {
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
            if (brands.IsNotEmpty())
            {
                var item = new CategoryFilterType();
                item.Position = 0;
                item.FilterTypeName = new FilterTypeName() { FilterType = FilterType.Brand, Text = Resource.Brands };
                for (int i = 0; i < brands.Count; i++)
                {
                    var brand = brands[i];
                    item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = string.Format("b{0}", brand.Id), name = brand.Name });
                }
                item.CategoryFilters.ForEach(r => r.Parent = item);
                categoryFilterTypes.Add(item);
            }
        }

        public void AddPriceFilter(List<CategoryFilterType> categoryFilterTypes)
        {
            if (categoryFilterTypes.IsNotEmpty())
            {
                return;
            }
            var item = new CategoryFilterType();
            item.Position = 1;
            item.FilterTypeName = new FilterTypeName() { FilterType = FilterType.Price, Text = Resource.Price };
            var item1 = new CategoryFilter()
            {
                CategoryFilterId = string.Format("p{0}", 100),
                minPrice = 0,
                maxPrice = 49
            };
            item1.name = string.Format("{0} {1}", item1.maxPrice.CurrencySign(), Resource.AndUnderPrice);
            item.CategoryFilters.Add(item1);

            item1 = new CategoryFilter()
            {
                CategoryFilterId = string.Format("p{0}", 101),
                minPrice = 49,
                maxPrice = 99
            };
            item1.name = string.Format("{0} - {1}", item1.minPrice.CurrencySign(), item1.maxPrice.CurrencySign());
            item.CategoryFilters.Add(item1);

            item1 = new CategoryFilter()
            {
                CategoryFilterId = string.Format("p{0}", 102),
                minPrice = 99,
                maxPrice = 499
            };
            item1.name = string.Format("{0} - {1}", item1.minPrice.CurrencySign(), item1.maxPrice.CurrencySign());
            item.CategoryFilters.Add(item1);

            item1 = new CategoryFilter()
            {
                CategoryFilterId = string.Format("p{0}", 103),
                minPrice = 499,
                maxPrice = 999
            };
            item1.name = string.Format("{0} - {1}", item1.minPrice.CurrencySign(), item1.maxPrice.CurrencySign());
            item.CategoryFilters.Add(item1);

            item1 = new CategoryFilter()
            {
                CategoryFilterId = string.Format("p{0}", 104),
                minPrice = 999,
                maxPrice = 4999
            };
            item1.name = string.Format("{0} - {1}", item1.minPrice.CurrencySign(), item1.maxPrice.CurrencySign());
            item.CategoryFilters.Add(item1);
          
            item1 = new CategoryFilter()
            {
                CategoryFilterId = string.Format("p{0}", 106),
                minPrice = 4999,
                maxPrice = 9999999
            };
            item1.name = string.Format("{0} {1}", item1.minPrice.CurrencySign(), Resource.AndOverPrice);
            item.CategoryFilters.Add(item1);
            item.CategoryFilters.ForEach(r => r.Parent = item);
            categoryFilterTypes.Add(item);
        }

        public void AddRatingFilter(List<CategoryFilterType> categoryFilterTypes)
        {
            CategoryFilterType item = new CategoryFilterType();
            item.Position = 5;
            item.FilterTypeName = new FilterTypeName() { FilterType = FilterType.Rating, Text = Resource.Rating };
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = string.Format("r{0}", 5), name = string.Format("5 {0}", Resource.Star), rating = 5 });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = string.Format("r{0}", 4), name = string.Format("4 {0}", Resource.Star), rating = 4 });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = string.Format("r{0}", 3), name = string.Format("3 {0}", Resource.Star), rating = 3 });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = string.Format("r{0}", 2), name = string.Format("2 {0}", Resource.Star), rating = 2 });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = string.Format("r{0}", 1), name = string.Format("1 {0}", Resource.Star), rating = 1 });
            item.CategoryFilters.ForEach(r => r.Parent = item);
            categoryFilterTypes.Add(item);
        }
    }
}