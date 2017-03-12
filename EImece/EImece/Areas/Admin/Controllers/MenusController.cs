using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EImece.Domain.Helpers.Extensions;


namespace EImece.Areas.Admin.Controllers
{
    public class MenusController : BaseAdminController
    {
        // GET: Admin/ProductCategories
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(String search = "")
        {
            Expression<Func<Menu, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var menus = MenuService.SearchEntities(whereLambda, search, CurrentLanguage);
            ViewBag.Tree = MenuService.CreateMenuTreeViewDataList(null, CurrentLanguage);
            ViewBag.MenuLeaves = MenuService.GetMenuLeaves(null, CurrentLanguage);
            return View(menus);
        }

        //
        // GET: /Menu/Details/5

        public ActionResult Details(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Menu content = MenuService.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }
            return View(content);
        }

        //
        // GET: /Menu/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = EntityFactory.GetBaseContentInstance<Menu>();
            ViewBag.Tree = MenuService.CreateMenuTreeViewDataList(null, CurrentLanguage);
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
        public ActionResult SaveOrEdit(Menu menu, HttpPostedFileBase menuImage = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    FilesHelper.SaveFileFromHttpPostedFileBase(menuImage,
                        menu.ImageHeight,
                        menu.ImageWidth,
                        EImeceImageType.MenuMainImage, menu);

                    menu.Lang = CurrentLanguage;
                    MenuService.SaveOrEditEntity(menu);
                    int contentId = menu.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.Message, menu);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator." + ex.Message.ToString());
            }
            ViewBag.Tree = MenuService.CreateMenuTreeViewDataList(null, CurrentLanguage);
            ViewBag.MenuLinks = GetMenuPages();
            return View(menu);
        }



        //
        [DeleteAuthorize()]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Menu content = MenuService.GetSingle(id);
            var menuTreeNodeList = MenuService.GetMenuLeaves(null, CurrentLanguage);
            var leave = menuTreeNodeList.FirstOrDefault(r => r.Id == id);

            if (content == null)
            {
                return HttpNotFound();
            }
            if (leave != null)
            {
                return View(content);
            }
            else
            {
                return Content("You cannot delete parent");
            }

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
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" +
                    ex.StackTrace, menu);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(menu);

        }



        public ActionResult GetMenus()
        {
            List<Menu> treelist = MenuService.BuildTree(null, CurrentLanguage);
            return new JsonResult { Data = new { treeList = treelist }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

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


            if (!menus.Any(r => r.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase)))
            {
                menuLinks.Add(new SelectListItem() { Text = "Ana Sayfa", Value = "home-index" });
            }
            if (!menus.Any(r => r.MenuLink.Equals("products-index", StringComparison.InvariantCultureIgnoreCase)))
            {
                menuLinks.Add(new SelectListItem() { Text = "Urunler", Value = "products-index" });
            }
            if (!menus.Any(r => r.MenuLink.Equals("stories-index", StringComparison.InvariantCultureIgnoreCase)))
            {
                menuLinks.Add(new SelectListItem() { Text = "Blogs", Value = "stories-index" });
            }
            menuLinks.Add(new SelectListItem() { Text = "Sayfalar", Value = "pages-index" });



            foreach (var storyCategory in storyCategories)
            {
                string m = "stories-categories_" + storyCategory.GetSeoUrl();
                if (!menus.Any(r => r.MenuLink.Equals(m, StringComparison.InvariantCultureIgnoreCase)))
                {
                    menuLinks.Add(new SelectListItem() { Text = storyCategory.Name, Value = m });
                }
            }





            return menuLinks;
        }
        public ActionResult ExportExcel()
        {
            String search = "";
            Expression<Func<Menu, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var menus = MenuService.SearchEntities(whereLambda, search, CurrentLanguage);
            DataTable dt = new DataTable();
            dt.TableName = "Menus";

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
            dt = GeneralHelper.LINQToDataTable(result);

            var ms = ExcelHelper.GetExcelByteArrayFromDataTable(dt);
            return File(ms, "application/vnd.ms-excel",
                String.Format("Menus-{0}-{1}.xls", GetCurrentLanguage,
                DateTime.Now.ToString("yyyy-MM-dd")));


        }
    }
}