using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using NLog;
using System;
using System.Data;
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
            Expression<Func<Setting, bool>> whereLambda = r => (r.SettingKey.ToLower().Contains(search.Trim().ToLower())
            || r.SettingValue.ToLower().Contains(search.Trim().ToLower()));

            var settings = SettingService.SearchEntities(whereLambda, search, CurrentLanguage);

            settings = settings.Where(r => !ApplicationConfigs.WebSiteLogo.Equals(r.SettingKey)).ToList();
            settings = settings.Where(r => !r.SettingValue.Equals(ApplicationConfigs.SpecialPage)).ToList();
            settings = settings.Where(r => !ApplicationConfigs.AdminSetting.Equals(r.Description)).ToList();
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
            var webSiteLogo = SettingService.GetSettingObjectByKey(ApplicationConfigs.WebSiteLogo);
            int id = webSiteLogo != null ? webSiteLogo.Id : 0;
            return RedirectToAction("WebSiteLogo", new { id });
        }
        public ActionResult AddTermsAndConditions()
        {
            var webSiteLogo = SettingService.GetSettingObjectByKey(ApplicationConfigs.TermsAndConditions, CurrentLanguage);
            int id = webSiteLogo != null ? webSiteLogo.Id : 0;
            return RedirectToAction("TermsAndConditions", new { id });
        }

        public ActionResult TermsAndConditions(int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Setting>();
            content.SettingKey = ApplicationConfigs.TermsAndConditions;
            content.Name = ApplicationConfigs.TermsAndConditions;
            content.Position = 1;
            content.IsActive = true;
            if (id == 0)
            {

            }
            else
            {
                content = SettingService.GetSingle(id);
                if (content.Lang != CurrentLanguage)
                {
                    return RedirectToAction("AddTermsAndConditions");
                }
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
                    setting.SettingValue = ApplicationConfigs.SpecialPage;
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
            var webSiteLogo = SettingService.GetSettingObjectByKey(ApplicationConfigs.PrivacyPolicy, CurrentLanguage);
            int id = webSiteLogo != null ? webSiteLogo.Id : 0;
            return RedirectToAction("PrivacyPolicy", new { id });
        }

        public ActionResult PrivacyPolicy(int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Setting>();
            content.SettingKey = ApplicationConfigs.PrivacyPolicy;
            content.Name = ApplicationConfigs.PrivacyPolicy;
            content.Position = 1;
            content.IsActive = true;
            if (id == 0)
            {

            }
            else
            {
                content = SettingService.GetSingle(id);
                if (content.Lang != CurrentLanguage)
                {
                    return RedirectToAction("AddPrivacyPolicy");
                }
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
                    setting.SettingValue = ApplicationConfigs.SpecialPage;
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
        public ActionResult UploadWebSiteLogo(int id = 0, int ImageWidth = 0, int ImageHeight = 0, HttpPostedFileBase postedImage = null)
        {

            var webSiteLogoSetting = EntityFactory.GetBaseEntityInstance<Setting>();
            if (id > 0)
            {
                webSiteLogoSetting = SettingService.GetSingle(id);
                FilesHelper.DeleteFile(webSiteLogoSetting.SettingValue);
                id = webSiteLogoSetting.Id;
            }

            var result = FilesHelper.SaveImageByte(ImageWidth, ImageHeight, postedImage);
            webSiteLogoSetting.Name = ApplicationConfigs.WebSiteLogo;
            webSiteLogoSetting.Description = "";
            webSiteLogoSetting.EntityHash = "";
            webSiteLogoSetting.SettingValue = result.NewFileName;
            webSiteLogoSetting.SettingKey = ApplicationConfigs.WebSiteLogo;
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

        public ActionResult ExportExcel()
        {
            String search = "";

            Expression<Func<Setting, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var settings = SettingService.SearchEntities(whereLambda, search, CurrentLanguage);


            var result = from r in settings
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             Name = r.Name.ToStr(250),
                             SettingKey = r.SettingKey,
                             SettingValue = r.SettingValue,
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                         };


            return DownloadFile(result, String.Format("Settings-{0}", GetCurrentLanguage));

        }
    }
}