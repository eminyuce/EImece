using EImece.Domain.Entities;
using SharkDev.Web.Controls.TreeView.Model;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IMenuRepository : IBaseContentRepository<Menu>, IDisposable
    {
        List<Menu> BuildTree(bool? isActive, int language);
        List<Node> CreateMenuTreeViewDataList(bool? isActive, int language);
        Menu GetMenuById(int menuId);
        List<Menu> GetMenuLeaves(bool? isActive, int language);

    }
}
