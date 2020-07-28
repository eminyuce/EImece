using EImece.Domain.Entities;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IMenuRepository : IBaseContentRepository<Menu>
    {
        List<Menu> BuildTree(bool? isActive, int language);

        Menu GetMenuById(int menuId);

        List<Menu> GetMenuLeaves(bool? isActive, int language);
    }
}