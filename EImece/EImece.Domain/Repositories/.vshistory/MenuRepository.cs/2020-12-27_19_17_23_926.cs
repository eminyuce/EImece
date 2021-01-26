using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Repositories
{
    public class MenuRepository : BaseContentRepository<Menu>, IMenuRepository
    {
        public MenuRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<MenuTreeModel> BuildTree(bool? isActive, int language)
        {
            List<Menu> list = GetActiveBaseContents(isActive, language);
            var returnList = new List<MenuTreeModel>();
            //find top levels items
            var topLevels = list.Where(a => a.ParentId == 0).OrderBy(r => r.Position).ToList();

            foreach (var i in topLevels)
            {
                var p = new MenuTreeModel();
                p.Menu = i;
                p.TreeLevel = 1;
                GetTreeview(list, p, p.TreeLevel);
                returnList.Add(p);
            }
            return returnList;
        }

        private void GetTreeview(List<Menu> list, MenuTreeModel current, int level)
        {
            //get child of current item
            var childs = list.Where(a => a.ParentId == current.Id).OrderBy(r => r.Position).ToList();
            if (childs.IsNotEmpty())
            {
                current.Childrens = new List<MenuTreeModel>();
                var childs2 = childs.Select(r => new MenuTreeModel(r, level + 1)).ToList();
                current.Childrens.AddRange(childs2);
                foreach (var i in childs)
                {
                    var p = new MenuTreeModel();
                    p.Menu = i;
                    p.Parent = current;
                    p.TreeLevel = level + 1;
                    GetTreeview(list, p, p.TreeLevel);
                }
            }
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