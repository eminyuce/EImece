using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IMenuRepository : IBaseContentRepository<Menu>
    {
        List<MenuTreeModel> BuildTree(bool? isActive, int language);

        Menu GetMenuById(int menuId);

        List<Menu> GetMenuLeaves(bool? isActive, int language);
    }
}