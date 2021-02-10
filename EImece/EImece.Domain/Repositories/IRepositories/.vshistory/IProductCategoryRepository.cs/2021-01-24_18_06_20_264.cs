using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IProductCategoryRepository : IBaseContentRepository<ProductCategory>
    {
        List<ProductCategoryTreeModel> BuildTree(bool? isActive, int language = 1);

        ProductCategory GetProductCategory(int categoryId, bool isOnlyActive = true);

        List<ProductCategory> GetProductCategoryLeaves(bool? isActive, int language);

        List<ProductCategory> GetMainPageProductCategories(int language);

        List<ProductCategory> GetAdminProductCategories(string search, int language);

        List<ProductCategory> GetProductCategoriesByParentId(int parentId);
        List<ProductCategoryTreeModel> BuildNavigation(bool? isActive, int language = 1);
    }
}