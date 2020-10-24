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
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class SettingsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult AddWebSiteLogo()
        {
            var webSiteLogo = SettingService.GetSettingObjectByKey(Constants.WebSiteLogo);
            if(webSiteLogo == null)
            {
                webSiteLogo = new Setting();
                webSiteLogo.SettingKey = Constants.WebSiteLogo;
            }
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

        public ActionResult UploadWebSiteLogo(int id = 0, int ImageWidth = 0, int ImageHeight = 0, HttpPostedFileBase postedImage = null)
        {
            if (postedImage != null)
            {
                var webSiteLogoSetting = EntityFactory.GetBaseEntityInstance<Setting>();
                if (id > 0)
                {
                    webSiteLogoSetting = SettingService.GetSingle(id);
                    FilesHelper.DeleteFile(webSiteLogoSetting.SettingValue);
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
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
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

            Expression<Func<Setting, bool>> whereLambda = r => r.Name.Contains(search);
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