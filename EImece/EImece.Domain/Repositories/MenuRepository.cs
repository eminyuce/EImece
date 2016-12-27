using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository;
using EImece.Domain.GenericRepositories;
using System.Data.Entity;
using SharkDev.Web.Controls.TreeView.Model;
using EImece.Domain.Helpers;

namespace EImece.Domain.Repositories
{
    public class MenuRepository : BaseContentRepository<Menu>, IMenuRepository
    {
        public MenuRepository(IEImeceContext dbContext) : base(dbContext)
        {
           
        }

        private void GetTreeview(List<Menu> list, Menu current, ref List<Menu> returnList)
        {
            //get child of current item
            var childs = list.Where(a => a.ParentId == current.Id).OrderBy(r => r.Position).ToList();
            current.Childrens = new List<Menu>();
            current.Childrens.AddRange(childs);
            foreach (var i in childs)
            {
                GetTreeview(list, i, ref returnList);
            }
        }

        public List<Menu> BuildTree()
        {
            List<Menu> list = GetAll().ToList();
            List<Menu> returnList = new List<Menu>();
            //find top levels items
            var topLevels = list.Where(a => a.ParentId == 0).OrderBy(r=>r.Position).ToList();
            returnList.AddRange(topLevels);
            foreach (var i in topLevels)
            {
                GetTreeview(list, i, ref returnList);
            }
            return returnList;
        }

        public List<Node> CreateMenuTreeViewDataList()
        {
            List<Node> _lstTreeNodes = new List<Node>();
            var menus = this.GetAll().OrderBy(r => r.Position).ToList();
            foreach (var p in menus)
            {
                _lstTreeNodes.Add(new Node() { Id = p.Id.ToStr(), Term = p.Name, ParentId = p.ParentId > 0 ? p.ParentId.ToStr() : "" });
            }

            return _lstTreeNodes;
        }
    }
}
