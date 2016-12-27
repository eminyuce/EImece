using EImece.Domain.Entities;
using EImece.Domain.Helpers;
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
            ViewBag.Tree = MenuService.CreateMenuTreeViewDataList();
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

            var content = new Menu();
            ViewBag.Tree = MenuService.CreateMenuTreeViewDataList();
            var parentMenu = new Menu();

           


            if (id == 0)
            {
                content.CreatedDate = DateTime.Now;
                content.IsActive = true;
                content.UpdatedDate = DateTime.Now;
                content.ParentId = 0;
            }
            else
            {

                content = MenuService.GetSingle(id);
                content.UpdatedDate = DateTime.Now;
                if(content.ParentId > 0)
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
                        var mainImage = ImageHelper.SaveFileFromHttpPostedFileBase(menuImage, 0, 0, EImeceImageType.MenuMainImage);
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
            ViewBag.Tree = MenuService.CreateMenuTreeViewDataList();
            return View(menu);
        }



        //
        // GET: /Menu/Delete/5
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
            List<Menu> treelist = MenuService.BuildTree();
            return new JsonResult { Data = new { treeList = treelist }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}