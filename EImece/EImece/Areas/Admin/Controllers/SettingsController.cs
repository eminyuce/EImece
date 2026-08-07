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
            if (webSiteLogo == null)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadWebSiteLogo(int id = 0, int ImageWidth = 0, int ImageHeight = 0, HttpPostedFileBase postedImage = null)
        {
            if (postedImage != null && postedImage.ContentLength > 0)
            {
                try
                {
                    var webSiteLogoSetting = EntityFactory.GetBaseEntityInstance<Setting>();
                    if (id > 0)
                    {
                        webSiteLogoSetting = SettingService.GetSingle(id);
                        if (webSiteLogoSetting == null)
                        {
                            return HttpNotFound();
                        }

                        // First-time logo (or cleared SettingValue) has nothing to delete.
                        if (!string.IsNullOrWhiteSpace(webSiteLogoSetting.SettingValue))
                        {
                            FilesHelper.DeleteFile(webSiteLogoSetting.SettingValue);
                        }
                    }

                    var result = FilesHelper.SaveImageByte(ImageWidth, ImageHeight, postedImage);
                    if (result == null || string.IsNullOrWhiteSpace(result.NewFileName))
                    {
                        ModelState.AddModelError("", "Logo kaydedilemedi. Lütfen geçerli bir görsel seçiniz.");
                        return View("WebSiteLogo", webSiteLogoSetting);
                    }

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
                    Logger.Info("Website logo uploaded. SettingId={0}, File={1}", webSiteLogoSetting.Id, result.NewFileName);
                    return View("WebSiteLogo", webSiteLogoSetting);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "UploadWebSiteLogo failed. id={0}, file={1}", id, postedImage.FileName);
                    ModelState.AddModelError("", "Logo yüklenirken hata oluştu: " + ex.Message);
                    var existing = id > 0 ? SettingService.GetSingle(id) : SettingService.GetSettingObjectByKey(Constants.WebSiteLogo);
                    return View("WebSiteLogo", existing ?? EntityFactory.GetBaseEntityInstance<Setting>());
                }
            }
            ModelState.AddModelError("", "Lütfen logo resmi seçiniz");
            var l = SettingService.GetSettingObjectByKey(Constants.WebSiteLogo);
            return View("WebSiteLogo", l ?? EntityFactory.GetBaseEntityInstance<Setting>());
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
                SetSuccessMessage();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, Setting);
                SetErrorMessage();
                return RedirectToAction("Index");
            }
        }

        public ActionResult ExportExcel(string format = "excel")
        {
            String search = "";

            Expression<Func<Setting, bool>> whereLambda = r => r.Name.Contains(search);
            var settings = SettingService.SearchEntities(whereLambda, search, CurrentLanguage);

            var result = from r in settings
                         select new
                         {
                             Id = r.Id,
                             Name = r.Name.ToStr(250),
                             SettingKey = r.SettingKey,
                             SettingValue = r.SettingValue,
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                         };

            return DownloadFile(result, String.Format("Settings-{0}", GetCurrentLanguage), format);
        }
    }
}