using EImece.Domain.Entities;
using SharkDev.Web.Controls.TreeView.Model;
using System.Collections.Generic;

namespace EImece.Domain.Models.AdminModels
{
    public class ProductAdminModel
    {
        public List<Product> Products { get; set; }
        public List<Node> ProductCategoryTree { get; set; }
        public Product Product { get; set; }
        public List<TagCategory> TagCategories { get; set; }
    }
}
