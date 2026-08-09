using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IProductCategoryService : IBaseContentService<ProductCategory>
    {
        List<ProductCategoryTreeModel> BuildTree(bool? isActive, int language = 1);

        ProductCategory GetProductCategory(int categoryId);

        Task<ProductCategory> GetProductCategoryAsync(int categoryId);

        List<ProductCategory> GetProductCategoryLeaves(bool? isActive, int language);

        void DeleteProductCategory(int productCategoryId);

        void DeleteProductCategories(List<string> values);

        ProductCategoryViewModel GetProductCategoryViewModel(int categoryId);

        Task<ProductCategoryViewModel> GetProductCategoryViewModelAsync(int categoryId);

        List<ProductCategory> GetMainPageProductCategories(int language);

        Task<List<ProductCategory>> GetMainPageProductCategoriesAsync(int language);

        List<ProductCategory> GetAdminProductCategories(string search, int currentLanguage);

        List<ProductCategoryTreeModel> GetBreadCrumb(int productCategoryId, int language);

        Task<List<ProductCategoryTreeModel>> GetBreadCrumbAsync(int productCategoryId, int language);

        List<ProductCategoryTreeModel> BuildNavigation(bool isActive, int currentLanguage);
        
        ProductCategoryDto GetProductCategoryDto(int productCategoryId);

        Task<ProductCategoryDto> GetProductCategoryDtoAsync(int productCategoryId);
    }
}