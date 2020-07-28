using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class NavigationModel
    {
        public List<Menu> Menus { get; set; }
        public List<ProductCategoryTreeModel> ProductCategories { get; set; }

        public NavigationModel(List<Menu> menus, List<ProductCategoryTreeModel> productCategories)
        {
            this.Menus = menus;
            this.ProductCategories = productCategories;
        }
    }
}