using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                var p = new MenuTreeModel(i, 1);
                GetTreeview(list, p, p.TreeLevel);
                returnList.Add(p);
            }
            return returnList;
        }

        private void GetTreeview(List<Menu> list, MenuTreeModel current, int level)
        {
            //get child of current item — recurse on the same instances added to Childrens
            // so nested levels (3+) are attached (previous code built orphans and dropped them).
            var childMenus = list.Where(a => a.ParentId == current.Id).OrderBy(r => r.Position).ToList();
            current.Childrens = new List<MenuTreeModel>();
            if (childMenus.IsEmpty())
            {
                return;
            }

            int childLevel = level + 1;
            foreach (var childMenu in childMenus)
            {
                var childNode = new MenuTreeModel(childMenu, childLevel)
                {
                    Parent = current
                };
                current.Childrens.Add(childNode);
                GetTreeview(list, childNode, childLevel);
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

        public List<Menu> GetMenus()
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MenuFiles.Select(t => t.FileStorage.FileStorageTags.Select(y => y.Tag)));
            includeProperties.Add(r => r.MainImage);
            return GetAllIncluding(includeProperties.ToArray()).ToList();
        }

        public async Task<List<Menu>> GetMenusAsync()
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MenuFiles.Select(t => t.FileStorage.FileStorageTags.Select(y => y.Tag)));
            includeProperties.Add(r => r.MainImage);
            return await GetAllIncluding(includeProperties.ToArray()).ToListAsync(CancellationToken.None).ConfigureAwait(false);
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