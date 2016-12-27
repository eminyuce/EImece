using EImece.Domain.Entities;
using SharkDev.Web.Controls.TreeView.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IMenuRepository : IBaseContentRepository<Menu>, IDisposable
    {
        List<Menu> BuildTree();
        List<Node> CreateMenuTreeViewDataList();
    }
}
