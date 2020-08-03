using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using NLog;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class SettingsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult AdminSettingPage(string pageName)
        {
            if (string.IsNullOrEmpty(pageName))
            {
                throw new ArgumentException("No pageName is defined.");
            }

            int settingId = GetSettingPage(pageName);
            if(settingId == 0)
            {
                var content = EntityFactory.GetBaseEntityInstance<Setting>();
                content.SettingKey = pageName;
                content.Name = pageName;
                content.Position = 1;
                content.IsActive = true;
                content.Lang = CurrentLanguage;
                return View(content);
            }
            else
            {
                var content = SettingService.GetSingle(settingId);
                return View(content);
            }
        }

        private int GetSettingPage(string id)
        {
            Setting setting = null;
            int settingId = 0;
            if (id.Equals(Constants.AboutUs, StringComparison.InvariantCultureIgnoreCase))
            {
                setting = SettingService.GetSettingObjectByKey(Constants.AboutUs, CurrentLanguage);
                settingId = setting != null ? setting.Id : 0;
                ViewBag.Title = Resource.AboutUs;
            }
            else if (id.Equals(Constants.DeliveryInfo, StringComparison.InvariantCultureIgnoreCase))
            {
                setting = SettingService.GetSettingObjectByKey(Constants.DeliveryInfo, CurrentLanguage);
                settingId = setting != null ? setting.Id : 0;
                ViewBag.Title = AdminResource.DeliveryInfo;
            }
            else if (id.Equals(Constants.TermsAndConditions, StringComparison.InvariantCultureIgnoreCase))
            {
                setting = SettingService.GetSettingObjectByKey(Constants.TermsAndConditions, CurrentLanguage);
                settingId = setting != null ? setting.Id : 0;
                ViewBag.Title = Resource.TermsAndConditions;
            }
            else if (id.Equals(Constants.PrivacyPolicy, StringComparison.InvariantCultureIgnoreCase))
            {
                setting = SettingService.GetSettingObjectByKey(Constants.PrivacyPolicy, CurrentLanguage);
                settingId = setting != null ? setting.Id : 0;
                ViewBag.Title = Resource.PrivacyPolicy;
            }
            else
            {
                throw new ArgumentException(id);
            }

            return settingId;
        }

        [HttpPost]
        public ActionResult AdminSettingPage(Setting setting)
        {
            try
            {
                if (setting == null)
                {
                    return HttpNotFound();
                }

              //  if (ModelState.IsValid)
                {
                    setting.SettingValue = Constants.SpecialPage;
                    setting.Lang = CurrentLanguage;
                    SettingService.SaveOrEditEntity(setting);
                    ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                }
             
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, setting);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            RemoveModelState();
             GetSettingPage(setting.SettingKey);
            return View("AdminSettingPage", setting);
        }
        public ActionResult AddWebSiteLogo()
        {
            var webSiteLogo = SettingService.GetSettingObjectByKey(Constants.WebSiteLogo);
            int id = webSiteLogo != null ? webSiteLogo.Id : 0;
            return RedirectToAction("WebSiteLogo", new { id });
        }

    
 

        public ActionResult AddTermsAndConditions()
        {
            var webSiteLogo = SettingService.GetSettingObjectByKey(Constants.TermsAndConditions, CurrentLanguage);
            int id = webSiteLogo != null ? webSiteLogo.Id : 0;
            return RedirectToAction("TermsAndConditions", new { id });
        }

        public ActionResult AddPrivacyPolicy()
        {
            var webSiteLogo = SettingService.GetSettingObjectByKey(Constants.PrivacyPolicy, CurrentLanguage);
            int id = webSiteLogo != null ? webSiteLogo.Id : 0;
            return RedirectToAction("PrivacyPolicy", new { id });
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
            if (postedImage != null)
            {
                var webSiteLogoSetting = EntityFactory.GetBaseEntityInstance<Setting>();
                if (id > 0)
                {
                    webSiteLogoSetting = SettingService.GetSingle(id);
                    FilesHelper.DeleteFile(webSiteLogoSetting.SettingValue);
                    id = webSiteLogoSetting.Id;
                }

                var result = FilesHelper.SaveImageByte(ImageWidth, ImageHeight, postedImage);
                webSiteLogoSetting.Name = Constants.WebSiteLogo;
                webSiteLogoSetting.Description = "";
                webSiteLogoSetting.SettingValue = result.NewFileName;
                webSiteLogoSetting.SettingKey = Constants.WebSiteLogo;
                webSiteLogoSetting.IsActive = true;
                webSiteLogoSetting.Position = 1;
                webSiteLogoSetting.Lang = CurrentLanguage;
                SettingService.SaveOrEditEntity(webSiteLogoSetting);
                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                RemoveModelState();
                return View("WebSiteLogo", webSiteLogoSetting);
            }
            ModelState.AddModelError("", "Lütfen logo resmi seçiniz");
            var l = SettingService.GetSettingObjectByKey(Constants.WebSiteLogo);
            return View("WebSiteLogo", l);
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
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
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