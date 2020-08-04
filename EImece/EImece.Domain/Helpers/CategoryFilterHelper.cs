using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using EImece.Domain.Models;
using EImece.Domain.Models.FrontModels;

namespace EImece.Domain.Helpers
{
    public class CategoryFilterHelper
    {
        private List<CategoryFilterType> categoryFilterTypes;
        private List<int> selectedFilters;

        public CategoryFilterHelper(List<CategoryFilterType> categoryFilterTypes, List<int> selectedFilters)
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
                    if (categoryFilter.CategoryFilters.Any(t => t.value == filterId))
                    {
                        var filterProperty = categoryFilter.CategoryFilters.FirstOrDefault(t => t.value == filterId);
                        switch (categoryFilter.FilterTypeName.FilterType)
                        {
                            case FilterType.Price:
                                filteredProducts.AddRange(products.Where(r => r.Price >= filterProperty.minPrice && r.Price < filterProperty.maxPrice).ToList());
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
                    if (categoryFilter.CategoryFilters.Any(t => t.value == filterId))
                    {
                        var filterProperty = categoryFilter.CategoryFilters.FirstOrDefault(t => t.value == filterId);
                        switch (categoryFilter.FilterTypeName.FilterType)
                        {
                            case FilterType.Rating:
                                filteredProducts.AddRange(products.Where(r => r.Rating >= filterProperty.value && r.Rating < filterProperty.value + 1).ToList());
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

        public static void addBrandFilter(List<CategoryFilterType> categoryFilterTypes)
        {
            var item = new CategoryFilterType();
            item.FilterTypeName = new FilterTypeName() { FilterType = FilterType.Brand, Text = "Brand" };
            item.CategoryFilters.Add(new CategoryFilter() { value = 100, name = "Apple" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 200, name = "Bosh" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 300, name = "Canon Inc." });
            item.CategoryFilters.Add(new CategoryFilter() { value = 400, name = "Dell" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 500, name = "Hewlett-Packard" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 600, name = "Hitachi" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 700, name = "LG Electronics" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 800, name = "Panasonic" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 900, name = "Sony" });
            categoryFilterTypes.Add(item);
        }

        public static void addPriceFilter(List<CategoryFilterType> categoryFilterTypes)
        {
            CategoryFilterType item = new CategoryFilterType();
            item.FilterTypeName = new FilterTypeName() { FilterType = FilterType.Price, Text = "Price" };
            item.CategoryFilters.Add(new CategoryFilter() { value = 1000, minPrice = 10, maxPrice = 50, name = "$10- $20" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 2000, minPrice = 50, maxPrice = 100, name = "$50 - $100" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 3000, minPrice = 100, maxPrice = 500, name = "$100 - $500" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 4000, minPrice = 500, maxPrice = 1000, name = "$500 - $1,000" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 5000, minPrice = 1000, maxPrice = 5000, name = "$1,000 - $5,000" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 6000, minPrice = 5000, maxPrice = 999999, name = "$5000 ve uzerinde" });
            categoryFilterTypes.Add(item);
        }

        public static void addRatingFilter(List<CategoryFilterType> categoryFilterTypes)
        {
            CategoryFilterType item = new CategoryFilterType();
            item.FilterTypeName = new FilterTypeName() { FilterType = FilterType.Rating, Text = "Rating" };
            item.CategoryFilters.Add(new CategoryFilter() { value = 1, name = "1 Yildiz" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 2, name = "2 Yildiz" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 3, name = "3 Yildiz" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 4, name = "4 Yildiz" });
            item.CategoryFilters.Add(new CategoryFilter() { value = 5, name = "5 Yildiz" });
            categoryFilterTypes.Add(item);
        }
    }
}
