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

            if (id == 0)
            {
                content.CreatedDate = DateTime.Now;
                content.IsActive = true;
                content.UpdatedDate = DateTime.Now;
            }
            else
            {

                content = MenuRepository.GetSingle(id);
                content.UpdatedDate = DateTime.Now;

            }

            return View(content);
        }

        //
        // POST: /Menu/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Menu menu)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (menu.Id == 0)
                    {
                        MenuRepository.Add(menu);
                    }
                    else
                    {
                        MenuRepository.Edit(menu);
                    }

                    MenuRepository.Save();
                    int contentId = menu.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, menu);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator."+ex.StackTrace.ToString());
            }

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

            Menu product = MenuRepository.GetSingle(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            try
            {
                MenuRepository.Delete(product);
                MenuRepository.Save();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, product);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(product);

        }
    }
}