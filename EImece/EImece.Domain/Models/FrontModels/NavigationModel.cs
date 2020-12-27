using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class NavigationModel
    {
        public List<MenuTreeModel> Menus { get; set; }
        public List<ProductCategoryTreeModel> ProductCategories { get; set; }

        public NavigationModel(List<MenuTreeModel> menus, List<ProductCategoryTreeModel> productCategories)
        {
            this.Menus = menus;
            this.ProductCategories = productCategories;
        }
    }
}