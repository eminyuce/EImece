using EImece.Web.Areas.Admin.Controllers;
using EImece.Domain;
using EImece.Web.Helpers;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Web.Filters;
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

using EImece.Domain.Factories.IFactories;
using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class SettingsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected IEntityFactory EntityFactory { get; }
        protected FilesHelper FilesHelper { get; }

        public SettingsController(
            ISettingService settingService,
            IEntityFactory entityFactory,
            FilesHelper filesHelper)
            : base(settingService)
        {
            EntityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
            FilesHelper = filesHelper ?? throw new ArgumentNullException(nameof(filesHelper));
        }

        public ActionResult Index()
        {
            return RedirectToAction("AddWebSiteLogo");
        }

        public ActionResult SystemSettings()
        {
            return RedirectToAction("SystemSettings", "AdminSettings");
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
            return RedirectToAction(Constants.WebSiteLogo, new { id });
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

        /// <summary>
        /// Upload is POST-only. A GET (e.g. after Refresh redirects to the prior form URL) must not 404.
        /// </summary>
        [HttpGet]
        public ActionResult UploadWebSiteLogo(int id = 0)
        {
            if (id > 0)
            {
                return RedirectToAction(Constants.WebSiteLogo, new { id });
            }

            return RedirectToAction("AddWebSiteLogo");
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
                        SetErrorMessage("Logo kaydedilemedi. Lütfen geçerli bir görsel seçiniz.");
                        return RedirectToLogoPage(id, webSiteLogoSetting);
                    }

                    webSiteLogoSetting.Name = Constants.WebSiteLogo;
                    webSiteLogoSetting.Description = "";
                    webSiteLogoSetting.SettingValue = result.NewFileName;
                    webSiteLogoSetting.SettingKey = Constants.WebSiteLogo;
                    webSiteLogoSetting.IsActive = true;
                    webSiteLogoSetting.Position = 1;
                    webSiteLogoSetting.Lang = CurrentLanguage;
                    await SettingService.SaveOrEditEntityAsync(webSiteLogoSetting);
                    Logger.Info("Website logo uploaded. SettingId={0}, File={1}", webSiteLogoSetting.Id, result.NewFileName);
                    SetSuccessMessage(AdminResource.SuccessfullySavedCompleted);
                    return RedirectToAction(Constants.WebSiteLogo, new { id = webSiteLogoSetting.Id });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "UploadWebSiteLogo failed. id={0}, file={1}", id, postedImage.FileName);
                    SetErrorMessage("Logo yüklenirken hata oluştu: " + ex.Message);
                    return RedirectToLogoPage(id, null);
                }
            }
            SetErrorMessage("Lütfen logo resmi seçiniz");
            return RedirectToLogoPage(id, null);
        }

        private ActionResult RedirectToLogoPage(int id, Setting setting)
        {
            var logoId = setting != null && setting.Id > 0 ? setting.Id : id;
            if (logoId > 0)
            {
                return RedirectToAction(Constants.WebSiteLogo, new { id = logoId });
            }

            return RedirectToAction("AddWebSiteLogo");
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
