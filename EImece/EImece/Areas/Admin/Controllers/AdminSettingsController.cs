using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Observability.Logging;
using EImece.Domain.Services.ExportImport;
using NLog;
using Resources;
using System;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using DomainConstants = EImece.Domain.Constants;

namespace EImece.Areas.Admin.Controllers
{
    public class AdminSettingsController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // GET: Admin/AdminSettings
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            SettingModel r = await SettingService.GetSettingModelAsync(CurrentLanguage);
            return View(r);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, SettingModel settingModel)
        {
            await SettingService.SaveSettingModelAsync(settingModel, CurrentLanguage);
            ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            return View(await SettingService.GetSettingModelAsync(CurrentLanguage));
        }

        public async Task<ActionResult> SystemSettings(CancellationToken cancellationToken)
        {
            SystemSettingModel r = await SettingService.GetSystemSettingModelAsync(cancellationToken);
            return View(r);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SystemSettings(CancellationToken cancellationToken, SystemSettingModel settingModel)
        {
            if (!string.IsNullOrWhiteSpace(settingModel.ProductPriceFilterSetting))
            {
                try
                {
                    var config = Newtonsoft.Json.JsonConvert.DeserializeObject<PriceFilterConfig>(settingModel.ProductPriceFilterSetting);
                    string error = null;
                    if (config == null || !config.IsValid(out error))
                    {
                        ModelState.AddModelError(nameof(settingModel.ProductPriceFilterSetting), error ?? "Geçersiz fiyat aralığı ayarı.");
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError(nameof(settingModel.ProductPriceFilterSetting), "Fiyat aralığı JSON formatı geçersiz.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(settingModel);
            }

            await SettingService.SaveSystemSettingModelAsync(settingModel);
            SetSuccessMessage(AdminResource.SuccessfullySavedCompleted);
            ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            return View(await SettingService.GetSystemSettingModelAsync(cancellationToken));
        }


        public async Task<ActionResult> SendSampleEmail(CancellationToken cancellationToken)
        {
            String companyName = "Testing company Name";
            var webSiteCompanyEmailAddress = await SettingService.GetSettingByKeyAsync(Constants.WebSiteCompanyEmailAddress);
            if (string.IsNullOrEmpty(webSiteCompanyEmailAddress))
            {
                ModelState.AddModelError("", AdminResource.WebSiteCompanyEmailAddressRequired);
                return View("SystemSettings", await SettingService.GetSystemSettingModelAsync(cancellationToken));
            }
            var emailAccount = await SettingService.GetEmailAccountAsync();
            var info = $"From-->{webSiteCompanyEmailAddress} {companyName} To: {emailAccount.ToString()}";
            try
            {
                string fromAddress = string.IsNullOrEmpty(emailAccount.Email) ? emailAccount.Username : emailAccount.Email;

                EmailSender.SendEmail(emailAccount,
                  subject: "Test Subject",
                  body: "Test Email Body",
                  fromAddress: fromAddress,
                  fromName: emailAccount.Username,
                  toAddress: webSiteCompanyEmailAddress,
                  toName: companyName);

                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            }
            catch (Exception ex)
            {
                Logger.Debug("It could not sent sample Email:" + info);
                ModelState.AddModelError("", ex.ToFormattedString());
            }

            return View("SystemSettings", await SettingService.GetSystemSettingModelAsync(cancellationToken));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeAdminPanelLanguage(string language, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return Json(new { success = false, message = "Language cannot be empty." });
            }

            var parsed = EnumHelper.ParseLanguage(language);
            var langValue = parsed.HasValue ? EnumHelper.GetEnumDescription(parsed.Value) : (language.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en-US" : "tr-TR");

            var setting = await SettingService.GetSettingObjectByKeyAsync(DomainConstants.AdminPanelLanguage).ConfigureAwait(false);
            if (setting == null)
            {
                setting = new EImece.Domain.Entities.Setting
                {
                    Name = DomainConstants.AdminPanelLanguage,
                    SettingKey = DomainConstants.AdminPanelLanguage,
                    Description = DomainConstants.SystemSettings,
                    IsActive = true,
                    SettingValue = langValue
                };
            }
            else
            {
                setting.SettingValue = langValue;
            }

            await SettingService.SaveOrEditEntityAsync(setting).ConfigureAwait(false);
            SettingService.ClearCache();
            MemoryCacheProvider?.ClearAll();

            return Json(new { success = true, language = langValue, message = AdminResource.SuccessfullySavedCompleted });
        }
    }
}
