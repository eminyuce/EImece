using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers.AttributeHelper;
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
    public class SettingsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActionResult Index(String search = "")
        {
            Expression<Func<Setting, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var settings = SettingService.SearchEntities(whereLambda, search);
            return View(settings);
        }

         
        //
        // GET: /Setting/Create

        public ActionResult SaveOrEdit(int id = 0)
        {

            var content = Setting.GetInstance<Setting>(); 


            if (id == 0)
            {

            }
            else
            {
                content = SettingService.GetSingle(id);
            }


            return View(content);
        }

        //
        // POST: /Setting/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Setting Setting)
        {
            try
            {

                if (ModelState.IsValid)
                {

                    SettingService.SaveOrEditEntity(Setting);
                    int contentId = Setting.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, Setting);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(Setting);
        }



        //
        [AuthorizeRoles(Settings.AdministratorRole)]
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Setting content = SettingService.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }


            return View(content);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles(Settings.AdministratorRole)]
        public ActionResult DeleteConfirmed(int id)
        {

            Setting Setting = SettingService.GetSingle(id);
            if (Setting == null)
            {
                return HttpNotFound();
            }
            try
            {
                SettingService.DeleteEntity(Setting);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, Setting);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(Setting);

        }
    }
}