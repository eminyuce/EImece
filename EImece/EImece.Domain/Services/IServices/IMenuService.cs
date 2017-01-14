using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using SharkDev.Web.Controls.TreeView.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IMenuService : IBaseContentService<Menu>
    {
        List<Menu> BuildTree(bool ? isActive, int language);
        List<Node> CreateMenuTreeViewDataList(bool? isActive, int language);
        MenuPageViewModel GetPageById(int menuId);
    }
}
