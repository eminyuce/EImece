using EImece.Domain;
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class MenusController : BaseAdminController
    {
        // GET: Admin/ProductCategories
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        public ActionResult Index(String search = "")
        {
            Expression<Func<Menu, bool>> whereLambda = r => r.Name.Contains(search);
            var menus = MenuService.SearchEntities(whereLambda, search, CurrentLanguage);
            ViewBag.MenuTree = MenuService.BuildTree(null, CurrentLanguage);
            ViewBag.MenuLeaves = MenuService.GetMenuLeaves(null, CurrentLanguage);
            return View(menus);
        }
        [HttpGet]
        public ActionResult MoveMenuCategory()
        {
            ViewBag.MenuCategoryDropDownList = GetMenuTreeDropDownList();
            ViewBag.MenuCategoryTree = MenuService.BuildTree(null, CurrentLanguage);
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveMenuCategory(MoveMenuCategory moveMenuCategory)
        {
            if (moveMenuCategory == null)
            {
                return HttpNotFound();
            }
            if (moveMenuCategory.FirstCategoryId > 0 && moveMenuCategory.SecondCategoryId > 0)
            {
                var firstCategoryId = MenuService.GetBaseContent(moveMenuCategory.FirstCategoryId);
                var secondCategory = MenuService.GetBaseContent(moveMenuCategory.SecondCategoryId);
                secondCategory.ParentId = firstCategoryId.Id;
                MenuService.SaveOrEditEntity(secondCategory);
            }
            else if (moveMenuCategory.SecondCategoryId > 0)
            {
                var secondCategory = MenuService.GetBaseContent(moveMenuCategory.SecondCategoryId);
                secondCategory.ParentId = 0;
                MenuService.SaveOrEditEntity(secondCategory);
            }
            return RedirectToAction("MoveMenuCategory");
        }
        private List<SelectListItem> GetMenuTreeDropDownList()
        {
            var resultListItem = new List<SelectListItem>();
            resultListItem.Add(new SelectListItem() { Text = AdminResource.MakeItRootCategory, Value = "0" });
            foreach (var item in MenuService.BuildTree(null, CurrentLanguage))
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
        //
        // GET: /Menu/Create

        public ActionResult SaveOrEdit(int id = 0)
        {
            var content = EntityFactory.GetBaseContentInstance<Menu>();
            ViewBag.MenuTree = MenuService.BuildTree(null, CurrentLanguage);
            ViewBag.MenuLinks = GetMenuPages();
            var parentMenu = EntityFactory.GetBaseContentInstance<Menu>();


            if (id == 0)
            {
                content.ParentId = 0;
            }
            else
            {
                content = MenuService.GetBaseContent(id);
                if (content.ParentId > 0)
                {
                    parentMenu = MenuService.GetSingle(content.ParentId);
                }
            }
            ViewBag.ParentMenu = parentMenu;

            return View(content);
        }

        //
        // POST: /Menu/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Menu menu, HttpPostedFileBase postedImage = null, String saveButton = null)
        {
            try
            {
                if (menu == null)
                {
                    return HttpNotFound();
                }

                if (ModelState.IsValid)
                {
                    FilesHelper.SaveFileFromHttpPostedFileBase(postedImage,
                        menu.ImageHeight,
                        menu.ImageWidth,
                        EImeceImageType.MenuMainImage, menu);

                    menu.Lang = CurrentLanguage;
                    MenuService.SaveOrEditEntity(menu);
                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Index");
                    }
                    else if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.Message, menu);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message.ToString());
            }
            ViewBag.MenuTree = MenuService.BuildTree(null, CurrentLanguage);
            ViewBag.MenuLinks = GetMenuPages();
          
            RemoveModelState();
            return View(menu);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            Menu menu = MenuService.GetSingle(id);

            if (menu == null)
            {
                return HttpNotFound();
            }
            try
            {
                MenuService.DeleteMenu(menu.Id);
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" +
                    ex.StackTrace, menu);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        public ActionResult GetMenus()
        {
            var treelist = MenuService.BuildTree(null, CurrentLanguage);
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

        private List<SelectListItem> GetMenuPages()
        {
            var menus = MenuService.GetActiveBaseContents(true, CurrentLanguage);
            var storyCategories = StoryCategoryService.GetActiveBaseContents(true, CurrentLanguage);
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
            //if (!menus.Any(r => r.MenuLink.Equals("stories-index", StringComparison.InvariantCultureIgnoreCase)))
            // {
            //     menuLinks.Add(new SelectListItem() { Text = "Blog Ana Sayfa", Value = "stories-index" });
            // }
            menuLinks.Add(new SelectListItem() { Text = "Farkli Sayfa Temalari", Value = "pages-index" });

           

            return menuLinks;
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync()
        {
            return await Task.Run(() =>
            {
                return DownloadFile();
            }).ConfigureAwait(true);
        }

        private ActionResult DownloadFile()
        {
            String search = "";
            Expression<Func<Menu, bool>> whereLambda = r => r.Name.Contains(search);
            var menus = MenuService.SearchEntities(whereLambda, search, CurrentLanguage);

            var result = from r in menus
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             ParentId = r.ParentId.ToStr(250),
                             Name = r.Name.ToStr(250),
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                             Description = r.Description,
                             MainPage = r.MainPage.ToStr(250),
                             ImageState = r.ImageState.ToStr(250),
                             MainImageId = r.MainImageId.ToStr(250)
                         };

            return DownloadFile(result, String.Format("Menus-{0}", GetCurrentLanguage));
        }
    }
}