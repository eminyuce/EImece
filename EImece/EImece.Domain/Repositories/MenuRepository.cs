using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class MenuRepository : BaseContentRepository<Menu>, IMenuRepository
    {
        public MenuRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        #region Storefront LINQ Projections & Read Methods

        private static Expression<Func<Menu, StorefrontPageDto>> PageProjection
        {
            get
            {
                return m => new StorefrontPageDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    MenuLink = m.MenuLink,
                    Description = m.Description,
                    ShortDescription = m.Description,
                    MainImageId = m.MainImageId,
                    MetaKeywords = m.MetaKeywords,
                    Position = m.Position,
                    Lang = m.Lang,
                    IsActive = m.IsActive,
                    UpdatedDate = m.UpdatedDate
                };
            }
        }

        private static Expression<Func<Menu, StorefrontMenuDto>> MenuProjection
        {
            get
            {
                return m => new StorefrontMenuDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    ParentId = m.ParentId,
                    MenuLink = m.MenuLink,
                    Url = m.Link,
                    Target = m.LinkIsActive ? "_blank" : "_self",
                    Description = m.Description,
                    ShortDescription = m.Description,
                    MainImageId = m.MainImageId,
                    Position = m.Position,
                    Lang = m.Lang,
                    IsActive = m.IsActive
                };
            }
        }

        public async Task<StorefrontPageDto> GetStorefrontPageByIdAsync(int menuId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Menus.AsNoTracking()
                .Where(m => m.Id == menuId && m.IsActive)
                .Select(PageProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public StorefrontPageDto GetStorefrontPageById(int menuId)
        {
            return EImeceDbContext.Menus.AsNoTracking()
                .Where(m => m.Id == menuId && m.IsActive)
                .Select(PageProjection)
                .FirstOrDefault();
        }

        public async Task<StorefrontPageDto> GetStorefrontPageByMenuLinkAsync(string menuLink, int? language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var query = EImeceDbContext.Menus.AsNoTracking()
                .Where(m => m.MenuLink == menuLink && m.IsActive);

            if (language.HasValue && language.Value > 0)
            {
                query = query.Where(m => m.Lang == language.Value);
            }

            return await query
                .Select(PageProjection)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public StorefrontPageDto GetStorefrontPageByMenuLink(string menuLink, int? language)
        {
            var query = EImeceDbContext.Menus.AsNoTracking()
                .Where(m => m.MenuLink == menuLink && m.IsActive);

            if (language.HasValue && language.Value > 0)
            {
                query = query.Where(m => m.Lang == language.Value);
            }

            return query
                .Select(PageProjection)
                .FirstOrDefault();
        }

        public async Task<List<StorefrontMenuDto>> GetStorefrontActiveMenusAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Menus.AsNoTracking()
                .Where(m => m.Lang == language && m.IsActive)
                .OrderBy(m => m.Position)
                .ThenByDescending(m => m.Id)
                .Select(MenuProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontMenuDto> GetStorefrontActiveMenus(int language)
        {
            return EImeceDbContext.Menus.AsNoTracking()
                .Where(m => m.Lang == language && m.IsActive)
                .OrderBy(m => m.Position)
                .ThenByDescending(m => m.Id)
                .Select(MenuProjection)
                .ToList();
        }

        public async Task<List<StorefrontMenuDto>> BuildStorefrontMenuTreeAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var allMenus = await GetStorefrontActiveMenusAsync(language, cancellationToken).ConfigureAwait(false);
            return BuildMenuHierarchy(allMenus);
        }

        public List<StorefrontMenuDto> BuildStorefrontMenuTree(int language)
        {
            var allMenus = GetStorefrontActiveMenus(language);
            return BuildMenuHierarchy(allMenus);
        }

        private static List<StorefrontMenuDto> BuildMenuHierarchy(List<StorefrontMenuDto> allMenus)
        {
            var topLevels = allMenus.Where(m => m.ParentId == 0).OrderBy(m => m.Position).ToList();
            foreach (var top in topLevels)
            {
                top.TreeLevel = 1;
                PopulateMenuChildren(allMenus, top, 1);
            }
            return topLevels;
        }

        private static void PopulateMenuChildren(List<StorefrontMenuDto> allMenus, StorefrontMenuDto current, int level)
        {
            var children = allMenus.Where(m => m.ParentId == current.Id).OrderBy(m => m.Position).ToList();
            current.Children = children;
            current.SideMenus = children;
            int childLevel = level + 1;
            foreach (var child in children)
            {
                child.TreeLevel = childLevel;
                PopulateMenuChildren(allMenus, child, childLevel);
            }
        }

        #endregion

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
            return menus.Where(m => !menus.Any(r => r.ParentId == m.Id)).ToList();
        }

        public async Task<List<MenuTreeModel>> BuildTreeAsync(bool? isActive, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<Menu> list = await GetActiveBaseContentsAsync(isActive, language, cancellationToken).ConfigureAwait(false);
            var returnList = new List<MenuTreeModel>();
            var topLevels = list.Where(a => a.ParentId == 0).OrderBy(r => r.Position).ToList();

            foreach (var i in topLevels)
            {
                var p = new MenuTreeModel(i, 1);
                GetTreeview(list, p, p.TreeLevel);
                returnList.Add(p);
            }
            return returnList;
        }

        public async Task<Menu> GetMenuByIdAsync(int menuId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MenuFiles.Select(t => t.FileStorage.FileStorageTags.Select(y => y.Tag)));
            includeProperties.Add(r => r.MainImage);
            return await GetSingleIncludingAsync(menuId, cancellationToken, includeProperties.ToArray()).ConfigureAwait(false);
        }

        public async Task<List<Menu>> GetMenuLeavesAsync(bool? isActive, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var menus = await GetActiveBaseContentsAsync(isActive, language, cancellationToken).ConfigureAwait(false);
            return menus.Where(m => !menus.Any(r => r.ParentId == m.Id)).ToList();
        }
    }
}