using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IMenuService : IBaseContentService<Menu>
    {
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