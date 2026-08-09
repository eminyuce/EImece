using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IProductCategoryRepository : IBaseContentRepository<ProductCategory>
    {
        List<ProductCategoryTreeModel> BuildTree(bool? isActive, int language = 1);

        Task<List<ProductCategoryTreeModel>> BuildTreeAsync(bool? isActive, int language = 1);

        ProductCategory GetProductCategory(int categoryId, bool isOnlyActive = true);

        Task<ProductCategory> GetProductCategoryAsync(int categoryId, bool isOnlyActive = true);

        List<ProductCategory> GetProductCategoryLeaves(bool? isActive, int language);

        List<ProductCategory> GetMainPageProductCategories(int language);

        Task<List<ProductCategory>> GetMainPageProductCategoriesAsync(int language);

        List<ProductCategory> GetAdminProductCategories(string search, int language);

        List<ProductCategory> GetProductCategoriesByParentId(int parentId);

        Task<List<ProductCategory>> GetProductCategoriesByParentIdAsync(int parentId);

        List<ProductCategoryTreeModel> BuildNavigation(bool? isActive, int language = 1);
    }
}