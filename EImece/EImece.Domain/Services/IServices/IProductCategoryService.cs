using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using SharkDev.Web.Controls.TreeView.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IProductCategoryService : IBaseContentService<ProductCategory>
    {
        List<ProductCategoryTreeModel> BuildTree(bool? isActive, int language = 1);
        List<Node> CreateProductCategoryTreeViewDataList(int language);
        ProductCategory GetProductCategory(int categoryId);
        List<ProductCategory> GetProductCategoryLeaves(bool? isActive, int language);
        void DeleteProductCategory(int productCategoryId);
        void DeleteProductCategories(List<string> values);
        List<ProductCategory> GetMainPageProductCategories(int language);
        List<ProductCategory> GetAdminProductCategories(string search, int currentLanguage);
    }
}
