using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Helpers
{
    public class CategoryFilterHelper
    {
        private List<CategoryFilterType> categoryFilterTypes;
        private List<int> selectedFilters;

        public CategoryFilterHelper()
        {
        }

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
                    if (categoryFilter.CategoryFilters.Any(t => t.CategoryFilterId == filterId))
                    {
                        var filterProperty = categoryFilter.CategoryFilters.FirstOrDefault(t => t.CategoryFilterId == filterId);
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
                    if (categoryFilter.CategoryFilters.Any(t => t.CategoryFilterId == filterId))
                    {
                        var filterProperty = categoryFilter.CategoryFilters.FirstOrDefault(t => t.CategoryFilterId == filterId);
                        switch (categoryFilter.FilterTypeName.FilterType)
                        {
                            case FilterType.Rating:
                                filteredProducts.AddRange(products.Where(r => r.Rating >= filterProperty.CategoryFilterId && r.Rating < filterProperty.CategoryFilterId + 1).ToList());
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

        public void addBrandFilter(List<CategoryFilterType> categoryFilterTypes)
        {
            var item = new CategoryFilterType();
            item.FilterTypeName = new FilterTypeName() { FilterType = FilterType.Brand, Text = "Brand" };
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 100, name = "Apple" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 200, name = "Bosh" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 300, name = "Canon Inc." });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 400, name = "Dell" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 500, name = "Hewlett-Packard" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 600, name = "Hitachi" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 700, name = "LG Electronics" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 800, name = "Panasonic" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 900, name = "Sony" });
            categoryFilterTypes.Add(item);
        }

        public void addPriceFilter(List<CategoryFilterType> categoryFilterTypes)
        {
            CategoryFilterType item = new CategoryFilterType();
            item.FilterTypeName = new FilterTypeName() { FilterType = FilterType.Price, Text = "Price" };
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 1000, minPrice = 10, maxPrice = 50, name = "$10- $20" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 2000, minPrice = 50, maxPrice = 100, name = "$50 - $100" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 3000, minPrice = 100, maxPrice = 500, name = "$100 - $500" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 4000, minPrice = 500, maxPrice = 1000, name = "$500 - $1,000" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 5000, minPrice = 1000, maxPrice = 5000, name = "$1,000 - $5,000" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 6000, minPrice = 5000, maxPrice = 999999, name = "$5000 ve uzerinde" });
            categoryFilterTypes.Add(item);
        }

        public void addRatingFilter(List<CategoryFilterType> categoryFilterTypes)
        {
            CategoryFilterType item = new CategoryFilterType();
            item.FilterTypeName = new FilterTypeName() { FilterType = FilterType.Rating, Text = "Rating" };
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 1, name = "1 Yildiz" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 2, name = "2 Yildiz" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 3, name = "3 Yildiz" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 4, name = "4 Yildiz" });
            item.CategoryFilters.Add(new CategoryFilter() { CategoryFilterId = 5, name = "5 Yildiz" });
            categoryFilterTypes.Add(item);
        }
    }
}