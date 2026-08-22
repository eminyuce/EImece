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
                    PageTheme = m.PageTheme,
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
                    LinkIsActive = m.LinkIsActive,
                    Description = m.Description,
                    ShortDescription = m.Description,
                    MainImageId = m.MainImageId,
                    Position = m.Position,
                    PageTheme = m.PageTheme
                };
            }
        }

        private static Expression<Func<Menu, StorefrontMenuNavigationDto>> MenuNavigationProjection
        {
            get
            {
                return m => new StorefrontMenuNavigationDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    ParentId = m.ParentId,
                    MenuLink = m.MenuLink,
                    Url = m.Link,
                    PageTheme = m.PageTheme,
                    Position = m.Position
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

        /// <summary>
        /// Two-column projection (Id, Name) of all active menus — used for slug lookups that
        /// need the computed SeoUrl but not the full row.
        /// </summary>
        public async Task<List<StorefrontMenuDto>> GetActiveMenuIdNamesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Menus.AsNoTracking()
                .Where(m => m.IsActive)
                .Select(m => new StorefrontMenuDto { Id = m.Id, Name = m.Name })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
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

        public async Task<List<StorefrontMenuNavigationDto>> GetStorefrontMenuNavigationAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.Menus.AsNoTracking()
                .Where(m => m.Lang == language && m.IsActive)
                .OrderBy(m => m.Position)
                .ThenByDescending(m => m.Id)
                .Select(MenuNavigationProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontMenuNavigationDto> GetStorefrontMenuNavigation(int language)
        {
            return EImeceDbContext.Menus.AsNoTracking()
                .Where(m => m.Lang == language && m.IsActive)
                .OrderBy(m => m.Position)
                .ThenByDescending(m => m.Id)
                .Select(MenuNavigationProjection)
                .ToList();
        }

        public async Task<List<StorefrontMenuNavigationDto>> BuildStorefrontMenuNavigationTreeAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var allMenus = await GetStorefrontMenuNavigationAsync(language, cancellationToken).ConfigureAwait(false);
            return BuildMenuNavigationHierarchy(allMenus);
        }

        public List<StorefrontMenuNavigationDto> BuildStorefrontMenuNavigationTree(int language)
        {
            var allMenus = GetStorefrontMenuNavigation(language);
            return BuildMenuNavigationHierarchy(allMenus);
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

        private static List<StorefrontMenuNavigationDto> BuildMenuNavigationHierarchy(List<StorefrontMenuNavigationDto> allMenus)
        {
            var topLevels = allMenus.Where(m => m.ParentId == 0).OrderBy(m => m.Position).ToList();
            foreach (var top in topLevels)
            {
                top.TreeLevel = 1;
                PopulateMenuNavigationChildren(allMenus, top, 1);
            }
            return topLevels;
        }

        private static void PopulateMenuNavigationChildren(List<StorefrontMenuNavigationDto> allMenus, StorefrontMenuNavigationDto current, int level)
        {
            var children = allMenus.Where(m => m.ParentId == current.Id).OrderBy(m => m.Position).ToList();
            current.Children = children;
            current.SideMenus = children;
            int childLevel = level + 1;
            foreach (var child in children)
            {
                child.TreeLevel = childLevel;
                PopulateMenuNavigationChildren(allMenus, child, childLevel);
            }
        }

        public async Task<List<StorefrontMenuFileDto>> GetStorefrontMenuFilesAsync(int menuId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await EImeceDbContext.MenuFiles.AsNoTracking()
                .Where(f => f.MenuId == menuId && f.FileStorage != null && f.FileStorage.IsActive)
                .OrderBy(f => f.FileStorage.Position)
                .Select(f => new StorefrontMenuFileDto
                {
                    Id = f.Id,
                    MenuId = f.MenuId,
                    FileStorageId = f.FileStorageId,
                    FileName = f.FileStorage.FileName,
                    Name = f.FileStorage.Name,
                    Position = f.FileStorage.Position,
                    IsActive = f.FileStorage.IsActive
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public List<StorefrontMenuFileDto> GetStorefrontMenuFiles(int menuId)
        {
            return EImeceDbContext.MenuFiles.AsNoTracking()
                .Where(f => f.MenuId == menuId && f.FileStorage != null && f.FileStorage.IsActive)
                .OrderBy(f => f.FileStorage.Position)
                .Select(f => new StorefrontMenuFileDto
                {
                    Id = f.Id,
                    MenuId = f.MenuId,
                    FileStorageId = f.FileStorageId,
                    FileName = f.FileStorage.FileName,
                    Name = f.FileStorage.Name,
                    Position = f.FileStorage.Position,
                    IsActive = f.FileStorage.IsActive
                })
                .ToList();
        }

        #endregion

        public List<MenuTreeModel> BuildTree(bool? isActive, int language)
        {
            var projectedMenus = GetProjectedMenus(isActive, language);
            var returnList = new List<MenuTreeModel>();
            //find top levels items
            var topLevels = projectedMenus.Where(a => a.ParentId == 0).OrderBy(r => r.Position).ToList();

            foreach (var i in topLevels)
            {
                var p = new MenuTreeModel(i, 1);
                GetProjectedTreeview(projectedMenus, p, p.TreeLevel);
                returnList.Add(p);
            }
            return returnList;
        }

        private List<StorefrontMenuDto> GetProjectedMenus(bool? isActive, int language)
        {
            var query = EImeceDbContext.Menus.AsNoTracking().Where(m => m.Lang == language);
            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }
            return query
                .OrderBy(m => m.Position)
                .Select(MenuProjection)
                .ToList();
        }

        private void GetProjectedTreeview(List<StorefrontMenuDto> list, MenuTreeModel current, int level)
        {
            //get child of current item — recurse on the same instances added to Childrens
            // so nested levels (3+) are attached.
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
                GetProjectedTreeview(list, childNode, childLevel);
            }
        }

        public Menu GetMenuById(int menuId)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MenuFiles.Select(t => t.FileStorage));
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
            var query = EImeceDbContext.Menus.AsNoTracking().Where(m => m.Lang == language);
            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }
            var list = await query
                .OrderBy(m => m.Position)
                .Select(MenuProjection)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            var returnList = new List<MenuTreeModel>();
            var topLevels = list.Where(a => a.ParentId == 0).OrderBy(r => r.Position).ToList();

            foreach (var i in topLevels)
            {
                var p = new MenuTreeModel(i, 1);
                GetProjectedTreeview(list, p, p.TreeLevel);
                returnList.Add(p);
            }
            return returnList;
        }

        public async Task<Menu> GetMenuByIdAsync(int menuId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.MenuFiles.Select(t => t.FileStorage));
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