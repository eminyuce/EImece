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

        //public ActionResult Index(String search = "")
        //{
        //    Expression<Func<Setting, bool>> whereLambda = r => (r.SettingKey.ToLower().Contains(search.Trim().ToLower())
        //    || r.SettingValue.ToLower().Contains(search.Trim().ToLower()));

        //    var settings = SettingService.SearchEntities(whereLambda, search, CurrentLanguage);

        //    settings = settings.Where(r => !Constants.WebSiteLogo.Equals(r.SettingKey)).ToList();
        //    settings = settings.Where(r => !r.SettingValue.Equals(Constants.SpecialPage)).ToList();
        //    settings = settings.Where(r => !Constants.AdminSetting.Equals(r.Description)).ToList();
        //    return View(settings);
        //}

        ////
        //// GET: /Setting/Create

        //public ActionResult SaveOrEdit(int id = 0)
        //{
        //    var content = EntityFactory.GetBaseEntityInstance<Setting>();

        //    if (id == 0)
        //    {
        //    }
        //    else
        //    {
        //        content = SettingService.GetSingle(id);
        //    }

        //    return View(content);
        //}

        ////
        //// POST: /Setting/Create

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult SaveOrEdit(Setting Setting)
        //{
        //    try
        //    {
        //        if (Setting == null)
        //        {
        //            return HttpNotFound();
        //        }
        //        if (ModelState.IsValid)
        //        {
        //            Setting.Lang = CurrentLanguage;
        //            SettingService.SaveOrEditEntity(Setting);
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, Setting);
        //        //Log the error (uncomment dex variable name and add a line here to write a log.
        //        ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
        //    }

        //    return View(Setting);
        //}

        public ActionResult AddWebSiteLogo()
        {
            var webSiteLogo = SettingService.GetSettingObjectByKey(Constants.WebSiteLogo);
            int id = webSiteLogo != null ? webSiteLogo.Id : 0;
            return RedirectToAction("WebSiteLogo", new { id });
        }

        public ActionResult AddAboutUs()
        {
            var setting = SettingService.GetSettingObjectByKey(Constants.AboutUs, CurrentLanguage);
            int id = setting != null ? setting.Id : 0;
            return RedirectToAction("AboutUs", new { id });
        }

        public ActionResult AboutUs(int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Setting>();
            content.SettingKey = Constants.AboutUs;
            content.Name = Constants.AboutUs;
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
                    return RedirectToAction("AboutUs");
                }
            }

            return View(content);
        }

        [HttpPost]
        public ActionResult AboutUs(Setting setting)
        {
            try
            {
                if (setting == null)
                {
                    return HttpNotFound();
                }

                if (ModelState.IsValid)
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
            return View(setting);
        }

        public ActionResult AddTermsAndConditions()
        {
            var webSiteLogo = SettingService.GetSettingObjectByKey(Constants.TermsAndConditions, CurrentLanguage);
            int id = webSiteLogo != null ? webSiteLogo.Id : 0;
            return RedirectToAction("TermsAndConditions", new { id });
        }

        public ActionResult TermsAndConditions(int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Setting>();
            content.SettingKey = Constants.TermsAndConditions;
            content.Name = Constants.TermsAndConditions;
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
                if (setting == null)
                {
                    return HttpNotFound();
                }

                if (ModelState.IsValid)
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
            return View(setting);
        }

        public ActionResult AddPrivacyPolicy()
        {
            var webSiteLogo = SettingService.GetSettingObjectByKey(Constants.PrivacyPolicy, CurrentLanguage);
            int id = webSiteLogo != null ? webSiteLogo.Id : 0;
            return RedirectToAction("PrivacyPolicy", new { id });
        }

        public ActionResult PrivacyPolicy(int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Setting>();
            content.SettingKey = Constants.PrivacyPolicy;
            content.Name = Constants.PrivacyPolicy;
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
                if (setting == null)
                {
                    return HttpNotFound();
                }

                // settings/privacypolicy/31/
                if (ModelState.IsValid)
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