using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class ProductCategoryViewModel
    {
        public ProductCategory ProductCategory { get; set; }

        public Menu ProductMenu { get; set; }
        public Menu MainPageMenu { get; set; }
        public List<ProductCategory> ChildrenProductCategories { get; set; }

        public List<ProductCategoryTreeModel> ProductCategoryTree { get; set; }
    }
}