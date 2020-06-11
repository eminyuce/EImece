using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using SharkDev.Web.Controls.TreeView.Model;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface IMenuService : IBaseContentService<Menu>
    {
        List<Menu> BuildTree(bool? isActive, int language);
        List<Node> CreateMenuTreeViewDataList(bool? isActive, int language);
        MenuPageViewModel GetPageById(int menuId);
        List<Menu> GetMenuLeaves(bool? isActive, int language);
        bool DeleteMenu(int menuId);
        void DeleteMenus(List<string> values);
        void UpdateStoryCategoryMenuLink(int storyCategoryId, int lang);
    }
}
