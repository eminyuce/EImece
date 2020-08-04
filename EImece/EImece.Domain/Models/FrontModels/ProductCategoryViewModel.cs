using EImece.Domain.Entities;
using EImece.Domain.Models.Enums;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductCategoryViewModel: ItemListing
    {
        public ProductCategory ProductCategory { get; set; }

        public Menu ProductMenu { get; set; }
        public Menu MainPageMenu { get; set; }
        public List<ProductCategory> ChildrenProductCategories { get; set; }

        public List<ProductCategoryTreeModel> ProductCategoryTree { get; set; }

        public List<Product> Products
        {
            get
            {
                List<Product> result = new List<Product>();
                ICollection<Product> products = ProductCategory.Products;

                if (!string.IsNullOrEmpty(Filter))
                {
                    foreach (var categoryFilter in CategoryFilterTypes)
                    {
                        foreach (var filterId in SelectedFilters)
                        {
                            if(categoryFilter.CategoryFilters.Any(t=>t.value == filterId))
                            {
                                var filterProperty = categoryFilter.CategoryFilters.FirstOrDefault(t => t.value == filterId);
                                switch (categoryFilter.FilterTypeName.FilterType)
                                {
                                    case FilterType.Brand:
                                        break;
                                    case FilterType.Price:
                                        result.AddRange(products.Where(r => r.Price >= filterProperty.minPrice && r.Price < filterProperty.maxPrice).ToList());
                                        break;
                                    case FilterType.Rating:
                                        result.AddRange(products.Where(r => r.Rating >= filterProperty.value).ToList());
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    result = products.ToList();
                }


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

        public string SeoId { get; set; }

        public List<CategoryFilterType> CategoryFilterTypes
        {
            get
            {
                var categoryFilterTypes = new List<CategoryFilterType>();
                addPriceFilter(categoryFilterTypes);
                addRatingFilter(categoryFilterTypes);
                return categoryFilterTypes;
            }
        }

        private static void addBrandFilter(List<CategoryFilterType> categoryFilterTypes)
        {
            var item = new CategoryFilterType();
            item.FilterTypeName = new FilterTypeName() { FilterType= FilterType.Brand, Text = "Brand" };
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

        private static void addPriceFilter(List<CategoryFilterType> categoryFilterTypes)
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

        private static void addRatingFilter(List<CategoryFilterType> categoryFilterTypes)
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