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
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class SettingsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            return RedirectToAction("AddWebSiteLogo");
        }

        public async Task<ActionResult> AddWebSiteLogo(CancellationToken cancellationToken)
        {
            var webSiteLogo = await SettingService.GetSettingObjectByKeyAsync(Constants.WebSiteLogo);
            if (webSiteLogo == null)
            {
                webSiteLogo = new Setting();
                webSiteLogo.SettingKey = Constants.WebSiteLogo;
            }
            int id = webSiteLogo != null ? webSiteLogo.Id : 0;
            return RedirectToAction("WebSiteLogo", new { id });
        }

        public async Task<ActionResult> WebSiteLogo(CancellationToken cancellationToken, int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Setting>();

            if (id == 0)
            {
            }
            else
            {
                content = await SettingService.GetSingleAsync(id);
            }

            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadWebSiteLogo(CancellationToken cancellationToken, int id = 0, int ImageWidth = 0, int ImageHeight = 0, HttpPostedFileBase postedImage = null)
        {
            if (postedImage != null && postedImage.ContentLength > 0)
            {
                try
                {
                    var webSiteLogoSetting = EntityFactory.GetBaseEntityInstance<Setting>();
                    if (id > 0)
                    {
                        webSiteLogoSetting = await SettingService.GetSingleAsync(id);
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
                    await SettingService.SaveOrEditEntityAsync(webSiteLogoSetting);
                    ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                    RemoveModelState();
                    Logger.Info("Website logo uploaded. SettingId={0}, File={1}", webSiteLogoSetting.Id, result.NewFileName);
                    return View("WebSiteLogo", webSiteLogoSetting);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "UploadWebSiteLogo failed. id={0}, file={1}", id, postedImage.FileName);
                    ModelState.AddModelError("", "Logo yüklenirken hata oluştu: " + ex.Message);
                    var existing = id > 0 ? await SettingService.GetSingleAsync(id) : await SettingService.GetSettingObjectByKeyAsync(Constants.WebSiteLogo);
                    return View("WebSiteLogo", existing ?? EntityFactory.GetBaseEntityInstance<Setting>());
                }
            }
            ModelState.AddModelError("", "Lütfen logo resmi seçiniz");
            var l = await SettingService.GetSettingObjectByKeyAsync(Constants.WebSiteLogo);
            return View("WebSiteLogo", l ?? EntityFactory.GetBaseEntityInstance<Setting>());
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Setting Setting = await SettingService.GetSingleAsync(id);
            if (Setting == null)
            {
                return HttpNotFound();
            }
            try
            {
                await SettingService.DeleteEntityAsync(Setting);
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

        public async Task<ActionResult> ExportExcel(CancellationToken cancellationToken, string format = "excel")
        {
            String search = "";

            Expression<Func<Setting, bool>> whereLambda = r => r.Name.Contains(search);
            var settings = await SettingService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);

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
