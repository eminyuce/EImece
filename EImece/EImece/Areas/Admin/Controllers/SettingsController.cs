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
            var settings = SettingService.SearchEntities(whereLambda, search, CurrentLanguage);
         
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
                    Setting.Lang = CurrentLanguage;
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
        public ActionResult AddTermsAndConditions()
        {
            var webSiteLogo = SettingService.GetSettingObjectByKey(Settings.TermsAndConditions);
            int id = webSiteLogo != null ? webSiteLogo.Id : 0;
            return RedirectToAction("TermsAndConditions", new { id });
        }

        public ActionResult TermsAndConditions(int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Setting>();
            content.SettingKey = Settings.TermsAndConditions;
            content.Name = Settings.TermsAndConditions;
            content.Position = 1;
            content.IsActive = true;
            if (id == 0)
            {

            }
            else
            {
                content = SettingService.GetSingle(id);
            }

            return View(content);
        }
        [HttpPost]
        public ActionResult TermsAndConditions(Setting setting)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    setting.Lang = CurrentLanguage;
                    SettingService.SaveOrEditEntity(setting);
                    int contentId = setting.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, setting);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(setting);
        }

        public ActionResult AddPrivacyPolicy()
        {
            var webSiteLogo = SettingService.GetSettingObjectByKey(Settings.PrivacyPolicy);
            int id = webSiteLogo != null ? webSiteLogo.Id : 0;
            return RedirectToAction("PrivacyPolicy", new { id });
        }

        public ActionResult PrivacyPolicy(int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Setting>();
            content.SettingKey = Settings.PrivacyPolicy;
            content.Name = Settings.PrivacyPolicy;
            content.Position = 1;
            content.IsActive = true;
            if (id == 0)
            {

            }
            else
            {
                content = SettingService.GetSingle(id);
            }

            return View(content);
        }
        [HttpPost]
        public ActionResult PrivacyPolicy(Setting setting)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    setting.Lang = CurrentLanguage;
                    SettingService.SaveOrEditEntity(setting);
                    int contentId = setting.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, setting);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(setting);
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
            webSiteLogoSetting.Lang = CurrentLanguage;
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