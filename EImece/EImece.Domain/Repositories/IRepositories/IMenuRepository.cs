using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IMenuRepository : IBaseContentRepository<Menu>
    {
        List<MenuTreeModel> BuildTree(bool? isActive, int language);

        Task<List<MenuTreeModel>> BuildTreeAsync(bool? isActive, int language, CancellationToken cancellationToken = default(CancellationToken));

        Menu GetMenuById(int menuId);

        Task<Menu> GetMenuByIdAsync(int menuId, CancellationToken cancellationToken = default(CancellationToken));

        List<Menu> GetMenuLeaves(bool? isActive, int language);

        Task<List<Menu>> GetMenuLeavesAsync(bool? isActive, int language, CancellationToken cancellationToken = default(CancellationToken));

        List<Menu> GetMenus();

        Task<List<Menu>> GetMenusAsync();

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        Task<StorefrontPageDto> GetStorefrontPageByIdAsync(int menuId, CancellationToken cancellationToken = default(CancellationToken));

        StorefrontPageDto GetStorefrontPageById(int menuId);

        Task<StorefrontPageDto> GetStorefrontPageByMenuLinkAsync(string menuLink, int? language, CancellationToken cancellationToken = default(CancellationToken));

        StorefrontPageDto GetStorefrontPageByMenuLink(string menuLink, int? language);

        Task<List<StorefrontMenuDto>> GetStorefrontActiveMenusAsync(int language, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontMenuDto> GetStorefrontActiveMenus(int language);

        Task<List<StorefrontMenuDto>> GetActiveMenuIdNamesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<List<StorefrontMenuNavigationDto>> GetStorefrontMenuNavigationAsync(int language, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontMenuNavigationDto> GetStorefrontMenuNavigation(int language);

        Task<List<StorefrontMenuNavigationDto>> BuildStorefrontMenuNavigationTreeAsync(int language, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontMenuNavigationDto> BuildStorefrontMenuNavigationTree(int language);

        Task<List<StorefrontMenuDto>> BuildStorefrontMenuTreeAsync(int language, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontMenuDto> BuildStorefrontMenuTree(int language);

        Task<List<StorefrontMenuFileDto>> GetStorefrontMenuFilesAsync(int menuId, CancellationToken cancellationToken = default(CancellationToken));

        List<StorefrontMenuFileDto> GetStorefrontMenuFiles(int menuId);

        #endregion
    }
}