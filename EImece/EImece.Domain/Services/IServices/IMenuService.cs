using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IMenuService : IBaseContentService<Menu>
    {
        MenuPageViewModel GetPageByMenuLink(string menuLink, int? language);

        Task<MenuPageViewModel> GetPageByMenuLinkAsync(string menuLink, int? language);

        List<MenuTreeModel> BuildTree(bool? isActive, int language);

        MenuPageViewModel GetPageById(int menuId);

        Task<MenuPageViewModel> GetPageByIdAsync(int menuId);

        List<Menu> GetMenuLeaves(bool? isActive, int language);

        bool DeleteMenu(int menuId);

        void DeleteMenus(List<string> values);

        void UpdateStoryCategoryMenuLink(int storyCategoryId, int lang);

        List<Menu> GetMenus();

        Task<List<Menu>> GetMenusAsync();
    }
}