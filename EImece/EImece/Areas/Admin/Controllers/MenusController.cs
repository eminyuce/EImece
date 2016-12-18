using EImece.Domain.Entities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var menus = MenuRepository.GetAll().ToList();
            if (!String.IsNullOrEmpty(search))
            {
                menus = menus.Where(r => r.Name.ToLower().Contains(search)).ToList();
            }

            ViewBag.Tree = CreateMenuTreeViewDataList();
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

            Menu content = MenuRepository.GetSingle(id);
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
            ViewBag.Tree = CreateMenuTreeViewDataList();
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

                content = MenuRepository.GetSingle(id);
                content.UpdatedDate = DateTime.Now;
                parentMenu = MenuRepository.GetSingle(content.ParentId);

            }
            ViewBag.ParentMenu = parentMenu;

            return View(content);
        }

        //
        // POST: /Menu/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Menu model)
        {
            try
            {
                ViewBag.Tree = CreateMenuTreeViewDataList();

                ViewBag.ParentMenu = MenuRepository.GetSingle(model.ParentId); ;
                if (ModelState.IsValid)
                {

                    MenuRepository.SaveOrEdit(model);
                    int contentId = model.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.Message, model);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator." + ex.Message.ToString());
            }

            return View(model);
        }



        //
        // GET: /Menu/Delete/5
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Menu content = MenuRepository.GetSingle(id);
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

            Menu menu = MenuRepository.GetSingle(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            try
            {
                MenuRepository.DeleteItem(menu);
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
            List<Menu> treelist = MenuRepository.BuildTree();
            return new JsonResult { Data = new { treeList = treelist }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}