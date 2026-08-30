using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.Abstractions;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Observability.Telemetry;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class MenuService : BaseContentService<Menu>, IMenuService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IStoryCategoryService StoryCategoryService;
        private readonly IMenuRepository MenuRepository;

        public MenuService(
            IMenuRepository repository,
            IEimeceCacheProvider dataCachingProvider,
            ISettingService settingService,
            IFileStorageService fileStorageService,
            ICurrentUserContext currentUserContext,
            FilesHelper filesHelper,
            IStoryCategoryService storyCategoryService)
            : base(repository, dataCachingProvider, settingService, fileStorageService, currentUserContext, filesHelper)
        {
            MenuRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            StoryCategoryService = storyCategoryService ?? throw new ArgumentNullException(nameof(storyCategoryService));
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        [Timed("service.menu.get_page_by_id")]

        public virtual async Task<StorefrontPageDto> GetStorefrontPageByIdAsync(int menuId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.MenuDetailAsync(menuId);
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => MenuRepository.GetStorefrontPageByIdAsync(menuId, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        [Timed("service.menu.get_page_by_id_sync")]
        public virtual StorefrontPageDto GetStorefrontPageById(int menuId)
        {
            var cacheKey = CacheKeys.MenuDetail(menuId);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => MenuRepository.GetStorefrontPageById(menuId),
                AppConfig.CacheLongSeconds);
        }

        [Timed("service.menu.get_page_by_link")]

        public virtual async Task<StorefrontPageDto> GetStorefrontPageByMenuLinkAsync(string menuLink, int? language, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Fixed links ("home-index", "products-index") are read on every product/category/story
            // page — cache under the menu: prefix so InvalidateMenuCaches drops them on save.
            var cacheKey = CacheKeys.MenuPrefix + "link:" + menuLink + ":lang" + (language.HasValue ? language.Value.ToString(CultureInfo.InvariantCulture) : "all") + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => MenuRepository.GetStorefrontPageByMenuLinkAsync(menuLink, language, CancellationToken.None),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        [Timed("service.menu.get_page_by_link_sync")]
        public virtual StorefrontPageDto GetStorefrontPageByMenuLink(string menuLink, int? language)
        {
            var cacheKey = CacheKeys.MenuPrefix + "link:" + menuLink + ":lang" + (language.HasValue ? language.Value.ToString(CultureInfo.InvariantCulture) : "all");
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => MenuRepository.GetStorefrontPageByMenuLink(menuLink, language),
                AppConfig.CacheLongSeconds);
        }

        [Timed("service.menu.get_active_menus", "Time taken to get active storefront menus")]
        public virtual async Task<List<StorefrontMenuDto>> GetStorefrontActiveMenusAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetStorefrontActiveMenusCachedAsync(language).ConfigureAwait(false);
        }

        /// <summary>
        /// Single-flight cached projected menu list (no entity materialization).
        /// </summary>
        [Timed("service.menu.get_active_cached")]

        public virtual async Task<List<StorefrontMenuDto>> GetStorefrontActiveMenusCachedAsync(int language)
        {
            var cacheKey = CacheKeys.MenuPrefix + "activemenus:lang" + language + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => MenuRepository.GetStorefrontActiveMenusAsync(language),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        public async Task<List<StorefrontMenuDto>> GetActiveMenuIdNamesAsync()
        {
            return await MenuRepository.GetActiveMenuIdNamesAsync().ConfigureAwait(false);
        }

        [Timed("service.menu.get_active_menus_sync")]
        public virtual List<StorefrontMenuDto> GetStorefrontActiveMenus(int language)
        {
            var cacheKey = CacheKeys.MenuPrefix + "activemenus:lang" + language;
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => MenuRepository.GetStorefrontActiveMenus(language),
                AppConfig.CacheLongSeconds);
        }

        [Timed("service.menu.build_tree")]

        public virtual async Task<List<StorefrontMenuDto>> BuildStorefrontMenuTreeAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.MenuTreeAsync(language);
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => MenuRepository.BuildStorefrontMenuTreeAsync(language, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        [Timed("service.menu.build_tree_sync")]
        public virtual List<StorefrontMenuDto> BuildStorefrontMenuTree(int language)
        {
            var cacheKey = CacheKeys.MenuTree(language);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => MenuRepository.BuildStorefrontMenuTree(language),
                AppConfig.CacheLongSeconds);
        }

        [Timed("service.menu.get_navigation", "Time taken to get storefront menu navigation")]
        public virtual async Task<List<StorefrontMenuNavigationDto>> GetStorefrontMenuNavigationAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.MenuPrefix + "nav:lang" + language + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => MenuRepository.GetStorefrontMenuNavigationAsync(language, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        [Timed("service.menu.get_navigation_sync")]
        public virtual List<StorefrontMenuNavigationDto> GetStorefrontMenuNavigation(int language)
        {
            var cacheKey = CacheKeys.MenuPrefix + "nav:lang" + language;
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => MenuRepository.GetStorefrontMenuNavigation(language),
                AppConfig.CacheLongSeconds);
        }

        [Timed("service.menu.build_nav_tree", "Time taken to build storefront menu navigation tree")]
        public virtual async Task<List<StorefrontMenuNavigationDto>> BuildStorefrontMenuNavigationTreeAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            // menu: family so InvalidateMenuCaches drops it (the former MenuNavTree-{lang}
            // key escaped every menu invalidation call).
            var cacheKey = CacheKeys.MenuPrefix + "navtree:lang" + language + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => MenuRepository.BuildStorefrontMenuNavigationTreeAsync(language, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        [Timed("service.menu.build_nav_tree_sync")]
        public virtual List<StorefrontMenuNavigationDto> BuildStorefrontMenuNavigationTree(int language)
        {
            var cacheKey = CacheKeys.MenuPrefix + "navtree:lang" + language;
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => MenuRepository.BuildStorefrontMenuNavigationTree(language),
                AppConfig.CacheLongSeconds);
        }

        /// <summary>
        /// Menu active-entity/content lists live under the menu: family so InvalidateMenuCaches evicts them.
        /// </summary>
        protected override string ActiveListCachePrefix
        {
            get { return CacheKeys.MenuPrefix; }
        }

        private void InvalidateMenuCaches()
        {
            DataCachingProvider.ClearByPrefix(CacheKeys.MenuPrefix);
            DataCachingProvider.Clear("GetMenus" + AsyncCacheKeySuffix);
            DataCachingProvider.Clear("GetMenus");
        }

        #endregion

        #region Mutation & Invalidation

        public override Menu SaveOrEditEntity(Menu entity)
        {
            var saved = base.SaveOrEditEntity(entity);
            InvalidateMenuCaches();
            return saved;
        }

        public override async Task<Menu> SaveOrEditEntityAsync(Menu entity)
        {
            var saved = await base.SaveOrEditEntityAsync(entity).ConfigureAwait(false);
            InvalidateMenuCaches();
            return saved;
        }

        #endregion

        #region Storefront Page ViewModels

        [Timed("service.menus.get_page_by_link_sync")]
        public virtual MenuPageViewModel GetPageByMenuLink(string menuLink, int? language)
        {
            var pageDto = MenuRepository.GetStorefrontPageByMenuLink(menuLink, language);
            if (pageDto == null)
            {
                return null;
            }
            return GetPageById(pageDto.Id);
        }

        [Timed("service.menus.get_page_by_link")]
        public virtual async Task<MenuPageViewModel> GetPageByMenuLinkAsync(string menuLink, int? language)
        {
            var pageDto = await MenuRepository.GetStorefrontPageByMenuLinkAsync(menuLink, language).ConfigureAwait(false);
            if (pageDto == null)
            {
                return null;
            }
            return await GetPageByIdAsync(pageDto.Id).ConfigureAwait(false);
        }

        [Timed("service.menus.get_page_by_id_sync")]
        public virtual MenuPageViewModel GetPageById(int pageId)
        {
            var pageDto = GetStorefrontPageById(pageId);
            if (pageDto == null) return null;

            var result = new MenuPageViewModel();
            result.Menu = new StorefrontMenuDto
            {
                Id = pageDto.Id,
                Name = pageDto.Name,
                MenuLink = pageDto.MenuLink,
                Description = pageDto.Description,
                ShortDescription = pageDto.ShortDescription,
                MainImageId = pageDto.MainImageId,
                Position = pageDto.Position,
                Lang = pageDto.Lang,
                PageTheme = pageDto.PageTheme,
                IsActive = pageDto.IsActive,
                MenuFiles = MenuRepository.GetStorefrontMenuFiles(pageId)
            };

            result.ApplicationSettings = SettingService.GetSettingKeyValues(pageDto.Lang);
            var allMenus = GetStorefrontActiveMenus(pageDto.Lang);
            result.SideMenus = allMenus.Where(m => m.ParentId == pageDto.Id || m.Id == pageDto.Id).ToList();

            var socialList = new Dictionary<string, string>();
            socialList.Add(Constants.InstagramWebSiteLink, SettingService.GetCachedSettingValue(Constants.InstagramWebSiteLink));
            socialList.Add(Constants.LinkedinWebSiteLink, SettingService.GetCachedSettingValue(Constants.LinkedinWebSiteLink));
            socialList.Add(Constants.YotubeWebSiteLink, SettingService.GetCachedSettingValue(Constants.YotubeWebSiteLink));
            socialList.Add(Constants.FacebookWebSiteLink, SettingService.GetCachedSettingValue(Constants.FacebookWebSiteLink));
            socialList.Add(Constants.TwitterWebSiteLink, SettingService.GetCachedSettingValue(Constants.TwitterWebSiteLink));
            socialList.Add(Constants.PinterestWebSiteLink, SettingService.GetCachedSettingValue(Constants.PinterestWebSiteLink));
            result.SocialMediaLinks = socialList;

            return result;
        }

        [Timed("service.menus.get_page_by_id")]
        public virtual async Task<MenuPageViewModel> GetPageByIdAsync(int pageId)
        {
            var pageDto = await GetStorefrontPageByIdAsync(pageId).ConfigureAwait(false);
            if (pageDto == null) return null;

            var menuFiles = await MenuRepository.GetStorefrontMenuFilesAsync(pageId).ConfigureAwait(false);

            var result = new MenuPageViewModel();
            result.Contact = new ContactUsFormViewModel();
            result.Menu = new StorefrontMenuDto
            {
                Id = pageDto.Id,
                Name = pageDto.Name,
                MenuLink = pageDto.MenuLink,
                Description = pageDto.Description,
                ShortDescription = pageDto.ShortDescription,
                MainImageId = pageDto.MainImageId,
                Position = pageDto.Position,
                Lang = pageDto.Lang,
                PageTheme = pageDto.PageTheme,
                IsActive = pageDto.IsActive,
                MenuFiles = menuFiles
            };

            result.ApplicationSettings = await SettingService.GetSettingKeyValuesAsync(pageDto.Lang).ConfigureAwait(false);

            var allMenus = await GetStorefrontActiveMenusAsync(pageDto.Lang).ConfigureAwait(false);
            result.SideMenus = allMenus.Where(m => m.ParentId == pageDto.Id || m.Id == pageDto.Id).ToList();

            var socialList = new Dictionary<string, string>();
            socialList.Add(Constants.InstagramWebSiteLink, SettingService.GetCachedSettingValue(Constants.InstagramWebSiteLink));
            socialList.Add(Constants.LinkedinWebSiteLink, SettingService.GetCachedSettingValue(Constants.LinkedinWebSiteLink));
            socialList.Add(Constants.YotubeWebSiteLink, SettingService.GetCachedSettingValue(Constants.YotubeWebSiteLink));
            socialList.Add(Constants.FacebookWebSiteLink, SettingService.GetCachedSettingValue(Constants.FacebookWebSiteLink));
            socialList.Add(Constants.TwitterWebSiteLink, SettingService.GetCachedSettingValue(Constants.TwitterWebSiteLink));
            socialList.Add(Constants.PinterestWebSiteLink, SettingService.GetCachedSettingValue(Constants.PinterestWebSiteLink));
            result.SocialMediaLinks = socialList;

            return result;
        }

        #endregion

        #region Admin Methods (Full Entities)

        public List<MenuTreeModel> BuildTree(bool? isActive, int language)
        {
            if (!IsCachingActivated)
            {
                return MenuRepository.BuildTree(isActive, language);
            }

            // menu: family so InvalidateMenuCaches drops it after menu edits.
            var cacheKey = CacheKeys.MenuPrefix + "admintree:" + isActive + ":lang" + language;
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

            var cacheKey = CacheKeys.MenuPrefix + "admintree:" + isActive + ":lang" + language + AsyncCacheKeySuffix;
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => MenuRepository.BuildTreeAsync(isActive, language, CancellationToken.None),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
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
                FileStorageService.DeleteGalleryImages(menuId, MediaModType.Menus);
                DeleteEntity(menu);
                InvalidateMenuCaches();

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
                await FileStorageService.DeleteGalleryImagesAsync(menuId, MediaModType.Menus).ConfigureAwait(false);
                await DeleteEntityAsync(menu).ConfigureAwait(false);
                InvalidateMenuCaches();

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
            var items = this.GetActiveBaseContentsFromCache(null, lang).Where(r1 => r1.MenuLink.Contains("stories-categories")).ToList();
            foreach (var item in items)
            {
                var menuLink = item.MenuLink;
                if (menuLink.GetId() == storyCategoryId)
                {
                    var storyCategory = StoryCategoryService.GetSingle(storyCategoryId);
                    string m = "stories-categories_" + storyCategory.GetSeoUrl();
                    item.MenuLink = m;
                    this.SaveOrEditEntity(item);
                }
            }
        }

        public async Task UpdateStoryCategoryMenuLinkAsync(int storyCategoryId, int lang)
        {
            var items = (await this.GetActiveBaseContentsFromCacheAsync(null, lang).ConfigureAwait(false)).Where(r1 => r1.MenuLink.Contains("stories-categories")).ToList();
            foreach (var item in items)
            {
                var menuLink = item.MenuLink;
                if (menuLink.GetId() == storyCategoryId)
                {
                    var storyCategory = await StoryCategoryService.GetSingleAsync(storyCategoryId).ConfigureAwait(false);
                    string m = "stories-categories_" + storyCategory.GetSeoUrl();
                    item.MenuLink = m;
                    await this.SaveOrEditEntityAsync(item).ConfigureAwait(false);
                }
            }
        }

        #endregion
    }
}