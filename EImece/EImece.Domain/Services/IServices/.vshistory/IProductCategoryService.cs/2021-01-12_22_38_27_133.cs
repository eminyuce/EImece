using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface IProductCategoryService : IBaseContentService<ProductCategory>
    {
        List<ProductCategoryTreeModel> BuildTree(bool? isActive, int language = 1);

        ProductCategory GetProductCategory(int categoryId);

        List<ProductCategory> GetProductCategoryLeaves(bool? isActive, int language);

        void DeleteProductCategory(int productCategoryId);

        void DeleteProductCategories(List<string> values);

        ProductCategoryViewModel GetProductCategoryViewModel(int categoryId);

        List<ProductCategory> GetMainPageProductCategories(int language);

        List<ProductCategory> GetAdminProductCategories(string search, int currentLanguage);

        List<ProductCategoryTreeModel> GetBreadCrumb(int productCategoryId, int language);

        ProductCategoryViewModel GetProductCategoryViewModelWithCache(int categoryId);


    }
}