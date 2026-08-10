using EImece.Domain.Entities;
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
    }
}