using EImece.Domain.Entities;
using SharkDev.Web.Controls.TreeView.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
