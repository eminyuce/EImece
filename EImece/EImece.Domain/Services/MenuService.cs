using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class MenuService : BaseContentService<Menu>, IMenuService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IStoryCategoryService StoryCategoryService { get; set; }

        private IMenuRepository MenuRepository { get; set; }

        public MenuService(IMenuRepository repository) : base(repository)
        {
            MenuRepository = repository;
        }

        public List<MenuTreeModel> BuildTree(bool? isActive, int language)
        {
            if (!IsCachingActivated)
            {
                return MenuRepository.BuildTree(isActive, language);
            }

            var cacheKey = String.Format("MenuTree-{0}-{1}", isActive, language);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => MenuRepository.BuildTree(isActive, language),
                AppConfig.CacheMediumSeconds);
        }

        public async Task<List<MenuTreeModel>> BuildTreeAsync(bool? isActive, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!IsCachingActivated)
            {
                return await MenuRepository.BuildTreeAsync(isActive, language, cancellationToken).ConfigureAwait(false);
            }

            var cacheKey = String.Format("MenuTree-{0}-{1}", isActive, language) + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => MenuRepository.BuildTreeAsync(isActive, language, CancellationToken.None),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        public MenuPageViewModel GetPageByMenuLink(string menuLink, int? language)
        {
            List<Menu> lists = GetMenus();
            var menu = lists.FirstOrDefault(r => (!language.HasValue || r.Lang == language.Value) && r.MenuLink.Equals(menuLink, StringComparison.InvariantCultureIgnoreCase));
            if (menu == null)
            {
                return null;
            }
            return GetPageById(menu.Id);
        }

        public async Task<MenuPageViewModel> GetPageByMenuLinkAsync(string menuLink, int? language)
        {
            List<Menu> lists = await GetMenusAsync().ConfigureAwait(false);
            var menu = lists.FirstOrDefault(r => (!language.HasValue || r.Lang == language.Value) && r.MenuLink.Equals(menuLink, StringComparison.InvariantCultureIgnoreCase));
            if (menu == null)
            {
                return null;
            }
            return await GetPageByIdAsync(menu.Id).ConfigureAwait(false);
        }

        public List<Menu> GetMenus()
        {
            if (!IsCachingActivated)
            {
                return MenuRepository.GetMenus();
            }

            var cacheKey = "GetMenus";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => MenuRepository.GetMenus(),
                AppConfig.CacheMediumSeconds);
        }

        public async Task<List<Menu>> GetMenusAsync()
        {
            if (!IsCachingActivated)
            {
                return await MenuRepository.GetMenusAsync().ConfigureAwait(false);
            }

            var cacheKey = "GetMenus" + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => MenuRepository.GetMenusAsync(),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        public MenuPageViewModel GetPageById(int menuId)
        {
            var menus = GetMenus();
            var menu = menus.FirstOrDefault(r => r.Id.Equals(menuId));
            if (menu == null)
            {
                Logger.Warn("GetPageById: menu id {0} was not found.", menuId);
                return null;
            }

            var result = new MenuPageViewModel();
            result.Contact = ContactUsFormViewModel.CreateContactUsFormViewModel("PageDetail", menuId, EImeceItemType.Menu);
            result.Menu = menu;
            result.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, menu.Lang).FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
            result.ApplicationSettings = SettingService.GetAllActiveSettings();  // SettingService.GetSettingObjectByKey(Settings.CompanyName);
            result.SocialMediaLinks = CreateMenuShareLinks(result.Menu);
            result.SideMenus = ResolveSideMenus(menu, menus);
            return result;
        }

        public async Task<MenuPageViewModel> GetPageByIdAsync(int menuId)
        {
            var menus = await GetMenusAsync().ConfigureAwait(false);
            var menu = menus.FirstOrDefault(r => r.Id.Equals(menuId));
            if (menu == null)
            {
                Logger.Warn("GetPageByIdAsync: menu id {0} was not found.", menuId);
                return null;
            }

            var result = new MenuPageViewModel();
            result.Contact = ContactUsFormViewModel.CreateContactUsFormViewModel("PageDetail", menuId, EImeceItemType.Menu);
            result.Menu = menu;
            var activeMenus = await MenuService.GetActiveBaseContentsFromCacheAsync(true, menu.Lang).ConfigureAwait(false);
            result.MainPageMenu = activeMenus.FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
            result.ApplicationSettings = await SettingService.GetAllActiveSettingsAsync().ConfigureAwait(false);
            result.SocialMediaLinks = CreateMenuShareLinks(result.Menu);
            result.SideMenus = ResolveSideMenus(menu, menus);
            return result;
        }

        private static List<Menu> ResolveSideMenus(Menu menu, IEnumerable<Menu> allMenus)
        {
            if (menu == null || allMenus == null)
            {
                return new List<Menu>();
            }

            var active = allMenus.Where(m => m != null && m.IsActive && m.Lang == menu.Lang);
            if (menu.ParentId > 0)
            {
                return active.Where(m => m.ParentId == menu.ParentId)
                    .OrderBy(m => m.Position)
                    .ThenBy(m => m.Name)
                    .ToList();
            }

            return active.Where(m => m.ParentId == menu.Id)
                .OrderBy(m => m.Position)
                .ThenBy(m => m.Name)
                .ToList();
        }

        private Dictionary<string, string> CreateMenuShareLinks(Menu menu)
        {
            if (menu == null)
            {
                return new Dictionary<string, string>();
            }

            var shareUrl = menu.GetDetailPageUrl("Detail", "Pages");
            var imageUrl = string.Empty;
            if (menu.MainImageId.HasValue)
            {
                imageUrl = menu.GetCroppedImageUrl(menu.MainImageId, 1000, 0, true) ?? string.Empty;
            }

            return SettingService.CreateShareableSocialMediaLinks(shareUrl, menu.Name, imageUrl);
        }

        public List<Menu> GetMenuLeaves(bool? isActive, int language)
        {
            return MenuRepository.GetMenuLeaves(isActive, language);
        }

        public async Task<List<Menu>> GetMenuLeavesAsync(bool? isActive, int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await MenuRepository.GetMenuLeavesAsync(isActive, language, cancellationToken).ConfigureAwait(false);
        }

        public bool DeleteMenu(int menuId)
        {
            var menu = MenuRepository.GetMenuById(menuId);
            var menuTreeNodeList = GetMenuLeaves(null, menu.Lang);
            var leave = menuTreeNodeList.FirstOrDefault(r => r.Id == menuId);
            if (leave != null)
            {
                if (menu.MainImageId.HasValue)
                {
                    FileStorageService.DeleteFileStorage(menu.MainImageId.Value);
                }
                if (menu.MenuFiles != null)
                {
                    var menuFiles = new List<MenuFile>(menu.MenuFiles);
                    foreach (var file in menuFiles)
                    {
                        FileStorageService.DeleteUploadImageByFileStorage(menuId, MediaModType.Menus, file.FileStorageId);
                    }
                    MenuFileRepository.DeleteByWhereCondition(r => r.MenuId == menuId);
                }
                DeleteEntity(menu);

                return true;
            }
            return false;
        }

        public async Task<bool> DeleteMenuAsync(int menuId)
        {
            var menu = await MenuRepository.GetMenuByIdAsync(menuId).ConfigureAwait(false);
            var menuTreeNodeList = await GetMenuLeavesAsync(null, menu.Lang).ConfigureAwait(false);
            var leave = menuTreeNodeList.FirstOrDefault(r => r.Id == menuId);
            if (leave != null)
            {
                if (menu.MainImageId.HasValue)
                {
                    await FileStorageService.DeleteFileStorageAsync(menu.MainImageId.Value).ConfigureAwait(false);
                }
                if (menu.MenuFiles != null)
                {
                    var menuFiles = new List<MenuFile>(menu.MenuFiles);
                    foreach (var file in menuFiles)
                    {
                        await FileStorageService.DeleteUploadImageByFileStorageAsync(menuId, MediaModType.Menus, file.FileStorageId).ConfigureAwait(false);
                    }
                    await MenuFileRepository.DeleteByWhereConditionAsync(r => r.MenuId == menuId).ConfigureAwait(false);
                }
                await DeleteEntityAsync(menu).ConfigureAwait(false);

                return true;
            }
            return false;
        }

        public void DeleteMenus(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    DeleteMenu(id);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                Logger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public async Task DeleteMenusAsync(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    await DeleteMenuAsync(id).ConfigureAwait(false);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                Logger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public void UpdateStoryCategoryMenuLink(int storyCategoryId, int lang)
        {
            var items = MenuService.GetActiveBaseContentsFromCache(null, lang).Where(r1 => r1.MenuLink.Contains("stories-categories")).ToList();
            foreach (var item in items)
            {
                var menuLink = item.MenuLink;
                if (menuLink.GetId() == storyCategoryId)
                {
                    var storyCategory = StoryCategoryService.GetSingle(storyCategoryId);
                    string m = "stories-categories_" + storyCategory.GetSeoUrl();
                    item.MenuLink = m;
                    MenuService.SaveOrEditEntity(item);
                }
            }
        }

        public async Task UpdateStoryCategoryMenuLinkAsync(int storyCategoryId, int lang)
        {
            var items = (await MenuService.GetActiveBaseContentsFromCacheAsync(null, lang).ConfigureAwait(false)).Where(r1 => r1.MenuLink.Contains("stories-categories")).ToList();
            foreach (var item in items)
            {
                var menuLink = item.MenuLink;
                if (menuLink.GetId() == storyCategoryId)
                {
                    var storyCategory = await StoryCategoryService.GetSingleAsync(storyCategoryId).ConfigureAwait(false);
                    string m = "stories-categories_" + storyCategory.GetSeoUrl();
                    item.MenuLink = m;
                    await MenuService.SaveOrEditEntityAsync(item).ConfigureAwait(false);
                }
            }
        }
    }
}