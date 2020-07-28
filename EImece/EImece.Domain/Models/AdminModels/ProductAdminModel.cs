using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;

namespace EImece.Domain.Models.AdminModels
{
    public class ProductAdminModel
    {
        public List<Product> Products { get; set; }
        public List<ProductCategoryTreeModel> ProductCategoryTree { get; set; }
        public Product Product { get; set; }
        public List<TagCategory> TagCategories { get; set; }
    }
}