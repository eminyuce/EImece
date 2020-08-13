using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using NPOI.OpenXmlFormats.Dml;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductCategoryViewModel : ItemListing
    {
        public ProductCategory ProductCategory { get; set; }

        public Menu ProductMenu { get; set; }
        public Menu MainPageMenu { get; set; }
        public List<ProductCategory> ChildrenProductCategories { get; set; }
        public List<Brand> Brands { get; set; }
        public List<ProductCategoryTreeModel> ProductCategoryTree { get; set; }
        public List<Product> Products
        {
            get
            {
                List<Product> result = new List<Product>();
                List<Product> products = ProductCategory.Products.ToList();
                if (MinPrice > 0 || MaxPrice > 0)
                {
                    if (MinPrice > 0 && MaxPrice > 0)
                    {
                        products = products.Where(r => r.PriceWithDiscount >= MinPrice && r.PriceWithDiscount < MaxPrice).ToList();
                    }
                    else if (MinPrice > 0)
                    {
                        products = products.Where(r => r.PriceWithDiscount >= MinPrice).ToList();
                    }
                    else if (MaxPrice > 0)
                    {
                        products = products.Where(r => r.PriceWithDiscount < MaxPrice).ToList();
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
                var categoryFilterHelper = new CategoryFilterHelper();
                categoryFilterHelper.AddPriceFilter(categoryFilterTypes);
                categoryFilterHelper.AddRatingFilter(categoryFilterTypes);
              var brandsWithProducts = 
                from t1 in ProductCategory.Products.ToList()
                join t2 in this.Brands on t1.BrandId equals t2.Id
                orderby t2.Position, t2.UpdatedDate 
                select t2;
                categoryFilterHelper.AddBrandFilter(categoryFilterTypes, brandsWithProducts.ToList());
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
            routeValues.Add("id", pagingItems.RouteId);
            routeValues.Add("sorting", pagingItems.Sorting);
            routeValues.Add("filtreler", pagingItems.Filter);
            if (pagingItems.MinPrice > 0)
            {
                routeValues.Add("minPrice", pagingItems.MinPrice);
            }
            if (pagingItems.MaxPrice > 0)
            {
                routeValues.Add("maxPrice", pagingItems.MaxPrice);
            }
            return routeValues;
        }
    }
}