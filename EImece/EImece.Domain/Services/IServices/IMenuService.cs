using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IMenuService : IBaseContentService<Menu>
    {
        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        Task<StorefrontPageDto> GetStorefrontPageByIdAsync(int menuId, CancellationToken cancellationToken = default(CancellationToken));
        StorefrontPageDto GetStorefrontPageById(int menuId);
        Task<StorefrontPageDto> GetStorefrontPageByMenuLinkAsync(string menuLink, int? language, CancellationToken cancellationToken = default(CancellationToken));
        StorefrontPageDto GetStorefrontPageByMenuLink(string menuLink, int? language);
        Task<List<StorefrontMenuDto>> GetStorefrontActiveMenusAsync(int language, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<StorefrontMenuDto>> GetStorefrontActiveMenusCachedAsync(int language);

        Task<List<StorefrontMenuDto>> GetActiveMenuIdNamesAsync();
        List<StorefrontMenuDto> GetStorefrontActiveMenus(int language);
        Task<List<StorefrontMenuNavigationDto>> GetStorefrontMenuNavigationAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
        List<StorefrontMenuNavigationDto> GetStorefrontMenuNavigation(int language);
        Task<List<StorefrontMenuNavigationDto>> BuildStorefrontMenuNavigationTreeAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
        List<StorefrontMenuNavigationDto> BuildStorefrontMenuNavigationTree(int language);
        Task<List<StorefrontMenuDto>> BuildStorefrontMenuTreeAsync(int language, CancellationToken cancellationToken = default(CancellationToken));
        List<StorefrontMenuDto> BuildStorefrontMenuTree(int language);

        #endregion

        MenuPageViewModel GetPageByMenuLink(string menuLink, int? language);

        Task<MenuPageViewModel> GetPageByMenuLinkAsync(string menuLink, int? language);

        List<MenuTreeModel> BuildTree(bool? isActive, int language);

        Task<List<MenuTreeModel>> BuildTreeAsync(bool? isActive, int language, CancellationToken cancellationToken = default(CancellationToken));

        MenuPageViewModel GetPageById(int menuId);

        Task<MenuPageViewModel> GetPageByIdAsync(int menuId);

        List<Menu> GetMenuLeaves(bool? isActive, int language);

        Task<List<Menu>> GetMenuLeavesAsync(bool? isActive, int language, CancellationToken cancellationToken = default(CancellationToken));

        bool DeleteMenu(int menuId);

        Task<bool> DeleteMenuAsync(int menuId);

        void DeleteMenus(List<string> values);

        Task DeleteMenusAsync(List<string> values);

        void UpdateStoryCategoryMenuLink(int storyCategoryId, int lang);

        Task UpdateStoryCategoryMenuLinkAsync(int storyCategoryId, int lang);

        List<Menu> GetMenus();

        Task<List<Menu>> GetMenusAsync();
    }
}