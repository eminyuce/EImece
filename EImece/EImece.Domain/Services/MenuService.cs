using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharkDev.Web.Controls.TreeView.Model;
using NLog;
using EImece.Domain.Models.FrontModels;
using System.Data.Entity.Validation;
using EImece.Domain.Helpers;

namespace EImece.Domain.Services
{
    public class MenuService : BaseContentService<Menu>, IMenuService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IMenuRepository MenuRepository { get; set; }
        public MenuService(IMenuRepository repository) : base(repository)
        {
            MenuRepository = repository;
        }

        public List<Menu> BuildTree(bool? isActive, int language)
        {
            var cacheKey = String.Format("MenuTree-{0}-{1}", isActive, language);
            List<Menu> result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = MenuRepository.BuildTree(isActive, language);
                MemoryCacheProvider.Set(cacheKey, result, Settings.CacheMediumSeconds);
            }
            return result;
        }

        public List<Node> CreateMenuTreeViewDataList(bool? isActive, int language)
        {
            return MenuRepository.CreateMenuTreeViewDataList(isActive, language);
        }

        public MenuPageViewModel GetPageById(int menuId)
        {
            var cacheKey = String.Format("GetPageById-{0}", menuId);
            MenuPageViewModel result = null;
            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = new MenuPageViewModel();
                result.Menu = MenuRepository.GetMenuById(menuId);
                MemoryCacheProvider.Set(cacheKey, result, Settings.CacheMediumSeconds);
            }
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
            {
                if (menu.MainImageId.HasValue)
                {
                    FileStorageService.DeleteFileStorage(menu.MainImageId.Value);
                }
                if (menu.MenuFiles != null)
                {
                    foreach (var file in menu.MenuFiles)
                    {
                        FileStorageService.DeleteFileStorage(file.FileStorageId);
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
    }
}
