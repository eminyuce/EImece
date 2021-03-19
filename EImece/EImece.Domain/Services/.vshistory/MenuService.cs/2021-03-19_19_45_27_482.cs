using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;

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
            List<MenuTreeModel> result = null;
            if (IsCachingActivated)
            {
                var cacheKey = String.Format("MenuTree-{0}-{1}", isActive, language);
                if (!MemoryCacheProvider.Get(cacheKey, out result))
                {
                    result = MenuRepository.BuildTree(isActive, language);
                    MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheMediumSeconds);
                }
            }
            else
            {
                result = MenuRepository.BuildTree(isActive, language);
            }

            return result;
        }

        public MenuPageViewModel GetPageByMenuLink(string menuLink, int? language)
        {
            List<Menu> lists = null;
            if (IsCachingActivated)
            {
                var cacheKey = String.Format("GetPageByMenuLink-{0}-{1}", menuLink, language);
                if (!MemoryCacheProvider.Get(cacheKey, out lists))
                {
                    lists = GetActiveBaseContents(true, language);
                    MemoryCacheProvider.Set(cacheKey, lists, AppConfig.CacheVeryLongSeconds);
                }
            }
            else
            {
                lists = GetActiveBaseContents(true, language);
            }

            var menu = lists.FirstOrDefault(r => r.MenuLink.Equals(menuLink, StringComparison.InvariantCultureIgnoreCase));
            if (menu == null)
            {
                return null;
            }
            return GetPageById(menu.Id);
        }

        public List<Menu> GetMenus()
        {
            var cacheKey = "GetMenus";
            List<Menu> result = null;
            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = MenuRepository.GetMenus();
                MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheMediumSeconds);
            }
            return result;
        }

        public MenuPageViewModel GetPageById(int menuId)
        {
                var result = new MenuPageViewModel();
                result.Contact = ContactUsFormViewModel.CreateContactUsFormViewModel("PageDetail", menuId, EImeceItemType.Menu);
                result.Menu = GetMenus().FirstOrDefault(r => r.Id.Equals(menuId));
                result.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, result.Menu.Lang).FirstOrDefault(r1 => r1.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
                result.ApplicationSettings = SettingService.GetAllActiveSettings();  // SettingService.GetSettingObjectByKey(Settings.CompanyName);
            return result;
        }

        public List<Menu> GetMenuLeaves(bool? isActive, int language)
        {
            return MenuRepository.GetMenuLeaves(isActive, language);
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
    }
}