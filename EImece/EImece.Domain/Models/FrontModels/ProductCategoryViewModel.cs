using EImece.Domain.Entities;
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
                List<Product> result = null;
                switch (Sorting)
                {
                    case Enums.SortingType.Popularity:
                        break;
                    case Enums.SortingType.LowHighPrice:
                        result = ProductCategory.Products.OrderBy(r => r.Price).ThenByDescending(r => r.Position).ThenByDescending(r => r.UpdatedDate).ToList();
                        break;
                    case Enums.SortingType.HighLowPrice:
                        result = ProductCategory.Products.OrderByDescending(r => r.Price).ThenByDescending(r => r.Position).ThenByDescending(r => r.UpdatedDate).ToList();
                        break;
                    case Enums.SortingType.AverageRating:
                        break;
                    case Enums.SortingType.AzOrder:
                        break;
                    case Enums.SortingType.ZaOrder:
                        break;
                    default:
                        result = ProductCategory.Products.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).ToList();
                        break;
                }

                return result;
               
            }
        }

        public string SeoId { get; set; }
    }
}