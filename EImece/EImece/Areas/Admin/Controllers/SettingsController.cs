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

            var content = EntityFactory.GetBaseEntityInstance<Setting>();


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
        public ActionResult AddWebSiteLogo()
        {
            var webSiteLogo = SettingService.GetSettingObjectByKey(Settings.WebSiteLogo);
            int id = webSiteLogo != null ? webSiteLogo.Id : 0;
            return RedirectToAction("WebSiteLogo", new { id });
        }
        public ActionResult WebSiteLogo(int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Setting>();

            if (id == 0)
            {

            }
            else
            {
                content = SettingService.GetSingle(id);
            }


            return View(content);
        }
        public ActionResult UploadWebSiteLogo(int id = 0, int ImageWidth = 0, int ImageHeight = 0, HttpPostedFileBase webSiteLogo = null)
        {

            var webSiteLogoSetting = EntityFactory.GetBaseEntityInstance<Setting>();
            if (id > 0)
            {
                webSiteLogoSetting = SettingService.GetSingle(id);
                FilesHelper.DeleteFile(webSiteLogoSetting.SettingValue);
                id = webSiteLogoSetting.Id;
            }
            
            var result = FilesHelper.SaveImageByte(ImageWidth, ImageHeight, webSiteLogo);
            webSiteLogoSetting.Name = Settings.WebSiteLogo;
            webSiteLogoSetting.Description = "";
            webSiteLogoSetting.EntityHash = "";
            webSiteLogoSetting.SettingValue = result.Item1;
            webSiteLogoSetting.SettingKey = Settings.WebSiteLogo;
            webSiteLogoSetting.IsActive = true;
            webSiteLogoSetting.Position = 1;
            SettingService.SaveOrEditEntity(webSiteLogoSetting);



            return RedirectToAction("WebSiteLogo", new { id = webSiteLogoSetting.Id });
        }
        //
        [DeleteAuthorize()]
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
        [DeleteAuthorize()]
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