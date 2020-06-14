using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using SharkDev.Web.Controls.TreeView.Model;
using System.Collections.Generic;
using System.Linq;

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

        public List<Menu> BuildTree(bool? isActive, int language)
        {
            List<Menu> list = GetActiveBaseContents(isActive, language);
            List<Menu> returnList = new List<Menu>();
            //find top levels items
            var topLevels = list.Where(a => a.ParentId == 0).OrderBy(r => r.Position).ToList();
            returnList.AddRange(topLevels);
            foreach (var i in topLevels)
            {
                GetTreeview(list, i, ref returnList);
            }
            return returnList;
        }

        public List<Node> CreateMenuTreeViewDataList(bool? isActive, int language)
        {
            List<Node> _lstTreeNodes = new List<Node>();
            List<Menu> menus = GetActiveBaseContents(isActive, language);
            foreach (var p in menus.OrderBy(r => r.Position))
            {
                _lstTreeNodes.Add(new Node() { Id = p.Id.ToStr(), Term = p.Name, ParentId = p.ParentId > 0 ? p.ParentId.ToStr() : "" });
            }

            return _lstTreeNodes;
        }

        public Menu GetMenuById(int menuId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MenuFiles.Select(t => t.FileStorage.FileStorageTags.Select(y => y.Tag)));
            includeProperties.Add(r => r.MainImage);
            var item = GetSingleIncluding(menuId, includeProperties.ToArray());

            return item;
        }

        public List<Menu> GetMenuLeaves(bool? isActive, int language)
        {
            var menus = GetActiveBaseContents(isActive, language);
            var result = new List<Menu>();

            foreach (var m in menus)
            {
                if (menus.Any(r => r.ParentId == m.Id))
                {
                    continue;
                }
                else
                {
                    result.Add(m);
                }
            }

            return result;
        }
    }
}