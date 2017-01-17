using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class MenusController : BaseAdminController
    {
        // GET: Admin/ProductCategories
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(String search = "")
        {
            Expression<Func<Menu, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var menus = MenuService.SearchEntities(whereLambda, search);
            ViewBag.Tree = MenuService.CreateMenuTreeViewDataList(null, Settings.MainLanguage);
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

            var content = Menu.GetInstance<Menu>();
            ViewBag.Tree = MenuService.CreateMenuTreeViewDataList(null, Settings.MainLanguage);
            ViewBag.MenuLinks = GetMenuPages();
            var parentMenu = Menu.GetInstance<Menu>();




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
                    if (menuImage != null)
                    {
                        var mainImage = FilesHelper.SaveFileFromHttpPostedFileBase(menuImage, 0, 0, EImeceImageType.MenuMainImage);
                        FileStorageService.SaveOrEditEntity(mainImage);
                        menu.MainImageId = mainImage.Id;
                    }

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
            ViewBag.Tree = MenuService.CreateMenuTreeViewDataList(null,Settings.MainLanguage);
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
            if (content == null)
            {
                return HttpNotFound();
            }


            return View(content);
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
                MenuService.DeleteEntity(menu);
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
            List<Menu> treelist = MenuService.BuildTree(null,Settings.MainLanguage);
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
            var menus = MenuService.GetAll();

            var menuLinks = new List<SelectListItem>();

            if (!menus.Any(r => r.MenuLink.Equals("products-index", StringComparison.InvariantCultureIgnoreCase)))
            {
                menuLinks.Add(new SelectListItem() { Text = "Urunler", Value = "products-index" });
            }

            if (!menus.Any(r => r.MenuLink.Equals("stories-index", StringComparison.InvariantCultureIgnoreCase)))
            {
                menuLinks.Add(new SelectListItem() { Text = "Blog", Value = "stories-index" });
            }
            menuLinks.Add(new SelectListItem() { Text = "Sayfalar", Value = "pages-index" });
            return menuLinks;
        }
    }
}