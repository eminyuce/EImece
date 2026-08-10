using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.AdminHelperModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class MenusController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<Menu, bool>> whereLambda = r => r.Name.Contains(search);
            var menus = await MenuService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            ViewBag.MenuTree = await MenuService.BuildTreeAsync(null, CurrentLanguage, cancellationToken);
            ViewBag.MenuLeaves = await MenuService.GetMenuLeavesAsync(null, CurrentLanguage, cancellationToken);
            return View(menus);
        }

        [HttpGet]
        public async Task<ActionResult> MoveMenuCategory(CancellationToken cancellationToken)
        {
            ViewBag.MenuCategoryDropDownList = await GetMenuTreeDropDownListAsync(cancellationToken);
            ViewBag.MenuCategoryTree = await MenuService.BuildTreeAsync(null, CurrentLanguage, cancellationToken);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveMenuCategory(MoveMenuCategory moveMenuCategory)
        {
            if (moveMenuCategory == null)
            {
                return HttpNotFound();
            }
            if (moveMenuCategory.FirstCategoryId > 0 && moveMenuCategory.SecondCategoryId > 0)
            {
                var firstCategoryId = await MenuService.GetBaseContentAsync(moveMenuCategory.FirstCategoryId);
                var secondCategory = await MenuService.GetBaseContentAsync(moveMenuCategory.SecondCategoryId);
                secondCategory.ParentId = firstCategoryId.Id;
                await MenuService.SaveOrEditEntityAsync(secondCategory);
            }
            else if (moveMenuCategory.SecondCategoryId > 0)
            {
                var secondCategory = await MenuService.GetBaseContentAsync(moveMenuCategory.SecondCategoryId);
                secondCategory.ParentId = 0;
                await MenuService.SaveOrEditEntityAsync(secondCategory);
            }
            return RedirectToAction("MoveMenuCategory");
        }

        private async Task<List<SelectListItem>> GetMenuTreeDropDownListAsync(CancellationToken cancellationToken)
        {
            var resultListItem = new List<SelectListItem>();
            resultListItem.Add(new SelectListItem() { Text = AdminResource.MakeItRootCategory, Value = "0" });
            foreach (var item in await MenuService.BuildTreeAsync(null, CurrentLanguage, cancellationToken))
            {
                resultListItem.Add(new SelectListItem() { Text = item.TextWithArrow, Value = item.Menu.Id.ToStr() });
                GetMenuTreeChildrenDropDownList(resultListItem, item);
            }

            return resultListItem;
        }

        private void GetMenuTreeChildrenDropDownList(List<SelectListItem> resultListItem, MenuTreeModel menuTreeModel)
        {
            if (menuTreeModel.Childrens.IsNotEmpty())
            {
                foreach (var item in menuTreeModel.Childrens)
                {
                    resultListItem.Add(new SelectListItem() { Text = item.TextWithArrow, Value = item.Menu.Id.ToStr() });
                    GetMenuTreeChildrenDropDownList(resultListItem, item);
                }
            }
        }

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var content = EntityFactory.GetBaseContentInstance<Menu>();
            ViewBag.MenuTree = await MenuService.BuildTreeAsync(null, CurrentLanguage, cancellationToken);
            ViewBag.MenuLinks = await GetMenuPagesAsync(cancellationToken);
            var parentMenu = EntityFactory.GetBaseContentInstance<Menu>();

            if (id == 0)
            {
                content.ParentId = 0;
            }
            else
            {
                content = await MenuService.GetBaseContentAsync(id, cancellationToken);
                if (content.ParentId > 0)
                {
                    parentMenu = await MenuService.GetSingleAsync(content.ParentId);
                }
            }
            ViewBag.ParentMenu = parentMenu;

            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, Menu menu, HttpPostedFileBase postedImage = null, String saveButton = null)
        {
            try
            {
                if (menu == null)
                {
                    return HttpNotFound();
                }

                if (menu != null && menu.MenuLink.Equals("-1"))
                {
                    ViewBag.MenuTree = await MenuService.BuildTreeAsync(null, CurrentLanguage, cancellationToken);
                    ViewBag.MenuLinks = await GetMenuPagesAsync(cancellationToken);
                    ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage);
                    ModelState.AddModelError("MenuLink", "Menu Link secimi yapiniz  ");
                    return View(menu);
                }

                if (ModelState.IsValid)
                {
                    FilesHelper.SaveFileFromHttpPostedFileBase(postedImage,
                        menu.ImageHeight,
                        menu.ImageWidth,
                        EImeceImageType.MenuMainImage, menu);

                    menu.Lang = CurrentLanguage;
                    await MenuService.SaveOrEditEntityAsync(menu);
                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Index");
                    }
                    else if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                    }
                }
                else
                {
                    ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.Message, menu);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message.ToString());
            }
            ViewBag.MenuTree = await MenuService.BuildTreeAsync(null, CurrentLanguage, cancellationToken);
            ViewBag.MenuLinks = await GetMenuPagesAsync(cancellationToken);

            RemoveModelState();
            return View(menu);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            Menu menu = await MenuService.GetSingleAsync(id);

            if (menu == null)
            {
                return HttpNotFound();
            }
            try
            {
                var deleted = await MenuService.DeleteMenuAsync(menu.Id);
                if (!deleted)
                {
                    SetErrorMessage(AdminResource.MenuCannotDeleteHasChildren);
                    return ReturnIndexIfNotUrlReferrer("Index");
                }
                SetSuccessMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete menu:" +
                    ex.StackTrace, menu);
                SetErrorMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetMenus(CancellationToken cancellationToken)
        {
            var treelist = await MenuService.BuildTreeAsync(null, CurrentLanguage, cancellationToken);
            return new JsonResult { Data = new { treeList = treelist }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public ActionResult Media(int id)
        {
            return RedirectToAction("Index", "Media", new
            {
                contentId = id,
                mod = MediaModType.Menus,
                imageType = EImeceImageType.MenuGallery
            });
        }

        private async Task<List<SelectListItem>> GetMenuPagesAsync(CancellationToken cancellationToken)
        {
            var menus = await MenuService.GetActiveBaseContentsAsync(true, CurrentLanguage, cancellationToken);
            var storyCategories = await StoryCategoryService.GetActiveBaseContentsAsync(true, CurrentLanguage, cancellationToken);
            var menuLinks = new List<SelectListItem>();
            menuLinks.Add(new SelectListItem() { Text = "Seçim Yapın", Value = "-1" });

            if (!menus.Any(r => r.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase)))
            {
                menuLinks.Add(new SelectListItem() { Text = "Ana Sayfa", Value = "home-index" });
            }
            if (!menus.Any(r => r.MenuLink.Equals("info-aboutus", StringComparison.InvariantCultureIgnoreCase)))
            {
                menuLinks.Add(new SelectListItem() { Text = "Hakkımızda", Value = "info-aboutus" });
            }
            if (!menus.Any(r => r.MenuLink.Equals("info-deliveryinfo", StringComparison.InvariantCultureIgnoreCase)))
            {
                menuLinks.Add(new SelectListItem() { Text = "Teslimat Bilgileri", Value = "info-deliveryinfo" });
            }
            if (!menus.Any(r => r.MenuLink.Equals("info-privacypolicy", StringComparison.InvariantCultureIgnoreCase)))
            {
                menuLinks.Add(new SelectListItem() { Text = "Şartlar ve Koşullar", Value = "info-privacypolicy" });
            }
            if (!menus.Any(r => r.MenuLink.Equals("info-termsandconditions", StringComparison.InvariantCultureIgnoreCase)))
            {
                menuLinks.Add(new SelectListItem() { Text = "Gizlilik Politikası", Value = "info-termsandconditions" });
            }
            if (!menus.Any(r => r.MenuLink.Equals("stories-index", StringComparison.InvariantCultureIgnoreCase)))
            {
                menuLinks.Add(new SelectListItem() { Text = "Blog Ana Sayfa", Value = "stories-index" });
            }
            menuLinks.Add(new SelectListItem() { Text = "Farkli Sayfa Temalari", Value = "pages-index" });

            return menuLinks;
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync(CancellationToken cancellationToken, string format = "excel")
        {
            return await DownloadFileAsync(format);
        }

        private async Task<ActionResult> DownloadFileAsync(string format = "excel")
        {
            String search = "";
            Expression<Func<Menu, bool>> whereLambda = r => r.Name.Contains(search);
            var menus = await MenuService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);

            var result = from r in menus
                         select new
                         {
                             Id = r.Id,
                             ParentId = r.ParentId,
                             Name = r.Name.ToStr(250),
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                             Description = r.Description,
                             MainPage = r.MainPage,
                             ImageState = r.ImageState,
                             MainImageId = r.MainImageId
                         };

            return DownloadFile(result, String.Format("Menus-{0}", GetCurrentLanguage), format);
        }
    }
}
