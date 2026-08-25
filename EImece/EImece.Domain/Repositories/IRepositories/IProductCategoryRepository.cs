using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;
using System.Threading;
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

        Task<List<ProductCategory>> GetProductCategoryLeavesAsync(bool? isActive, int language, CancellationToken cancellationToken = default(CancellationToken));

        List<ProductCategory> GetMainPageProductCategories(int language);

        Task<List<ProductCategory>> GetMainPageProductCategoriesAsync(int language);

        List<ProductCategory> GetAdminProductCategories(string search, int language);

        Task<List<ProductCategory>> GetAdminProductCategoriesAsync(string search, int language, CancellationToken cancellationToken = default(CancellationToken));

        List<ProductCategory> GetProductCategoriesByParentId(int parentId);

        Task<List<ProductCategory>> GetProductCategoriesByParentIdAsync(int parentId);

        List<ProductCategoryTreeModel> BuildNavigation(bool? isActive, int language = 1);

        Task<List<ProductCategory>> GetProductCategoriesForImageExportAsync(CancellationToken cancellationToken = default(CancellationToken));

        List<ProductCategory> GetProductCategoriesForImageExport();

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        Task<StorefrontCategoryDto> GetStorefrontCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default(CancellationToken));

        StorefrontCategoryDto GetStorefrontCategoryById(int categoryId);

        ProductCategoryDto GetProductCategoryDto(int categoryId);

        Task<ProductCategoryDto> GetProductCategoryDtoAsync(int categoryId, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<StorefrontCategoryDto>> GetStorefrontMainPageCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontCategoryDto> GetStorefrontMainPageCategories(int language);

        Task<List<StorefrontCategoryDto>> GetStorefrontChildrenCategoriesAsync(int parentId, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontCategoryDto> GetStorefrontChildrenCategories(int parentId);

        Task<List<StorefrontCategoryDto>> BuildStorefrontNavigationTreeAsync(int language, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontCategoryDto> BuildStorefrontNavigationTree(int language);

        #endregion
    }
}