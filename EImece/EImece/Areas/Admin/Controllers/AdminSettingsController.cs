using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Services.ExportImport;
using EImece.Domain.Services.IServices;
using EImece.Web.Areas.Admin.Controllers;
using Microsoft.Extensions.Logging;
using Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using DomainConstants = EImece.Domain.Constants;

namespace EImece.Areas.Admin.Controllers
{
    public class AdminSettingsController : BaseAdminController
    {
        private readonly IEmailSender _emailSender;
        private readonly IEimeceCacheProvider _memoryCacheProvider;
        private readonly IDataExportService _dataExportService;

        public AdminSettingsController(ISettingService settingService,
            IEmailSender emailSender,
            IEimeceCacheProvider memoryCacheProvider,
            IDataExportService dataExportService, ILogger<AdminSettingsController> logger)
            : base(settingService, logger)
        {
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _memoryCacheProvider = memoryCacheProvider ?? throw new ArgumentNullException(nameof(memoryCacheProvider));
            _dataExportService = dataExportService ?? throw new ArgumentNullException(nameof(dataExportService));
        }

        // GET: Admin/AdminSettings
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            SettingModel r = await SettingService.GetSettingModelAsync(CurrentLanguage);
            return View(r);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
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
        [ValidateInput(false)]
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

            if (!settingModel.ContentLanguageTurkish && !settingModel.ContentLanguageEnglish)
            {
                ModelState.AddModelError(nameof(settingModel.SupportedContentLanguages), AdminResource.ContentLanguagesAtLeastOne);
            }
            else
            {
                settingModel.SupportedContentLanguages = ContentLanguageSettingsHelper.Serialize(
                    settingModel.ContentLanguageTurkish,
                    settingModel.ContentLanguageEnglish);
            }

            if (!ModelState.IsValid)
            {
                return View(settingModel);
            }

            await SettingService.SaveSystemSettingModelAsync(settingModel);
            _memoryCacheProvider.ClearAll();
            SetSuccessMessage(AdminResource.SuccessfullySavedCompleted);
            ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            return View(await SettingService.GetSystemSettingModelAsync(cancellationToken));
        }


        public async Task<ActionResult> SendSampleEmail(CancellationToken cancellationToken)
        {
            String companyName = "Testing company Name";
            var webSiteCompanyEmailAddress = await SettingService.GetSettingByKeyFromDbAsync(Constants.WebSiteCompanyEmailAddress);
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

                _emailSender.SendEmail(emailAccount,
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
                Logger.LogDebug("It could not sent sample Email:" + info);
                ModelState.AddModelError("", ex.ToFormattedString());
            }

            return View("SystemSettings", await SettingService.GetSystemSettingModelAsync(cancellationToken));
        }

        /// <summary>
        /// Streams a full database backup as a ZIP archive containing one JSON file per entity.
        /// Triggered by the "Download Complete JSON Backup" button on the system settings tools tab.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ExportBackup(CancellationToken cancellationToken)
        {
            Logger.LogDebug("Full database JSON backup export requested.");
            var exportRequest = new DataExportRequest
            {
                ExportedBy = $"{User?.Identity?.Name ?? "Admin"}"
            };

            using (var outputStream = new MemoryStream())
            {
                DataExportResult result = await _dataExportService.ExportDataAsync(exportRequest, outputStream, cancellationToken).ConfigureAwait(false);
                if (result == null || !result.Success)
                {
                    var errorMessage = result?.ErrorMessage ?? AdminResource.Error;
                    Logger.LogError("Database JSON backup export failed: {0}", errorMessage);
                    ModelState.AddModelError("", errorMessage);
                    return View("SystemSettings", await SettingService.GetSystemSettingModelAsync(cancellationToken));
                }

                Logger.LogInformation("Database JSON backup generated: {0} records, {1} bytes.", result.TotalRecords, result.CompressedSizeBytes);
                string fileName = $"eimece-db-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";
                string contentType = MediaTypeNames.Application.Zip;
                return File(outputStream.ToArray(), contentType, fileName);
            }
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

            var setting = await SettingService.GetSettingObjectByKeyFromDbAsync(DomainConstants.AdminPanelLanguage).ConfigureAwait(false);
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
            _memoryCacheProvider?.ClearAll();

            return Json(new { success = true, language = langValue, message = AdminResource.SuccessfullySavedCompleted });
        }

        [HttpGet]
        public async Task<ActionResult> GetPageHelpList(CancellationToken cancellationToken)
        {
            var dict = await PageHelpHelper.GetAllHelpTextsAsync(SettingService, _memoryCacheProvider).ConfigureAwait(false);
            var items = dict != null ? dict.Values.ToList() : new List<PageHelpItemDto>();
            return Json(new { success = true, items = items }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetPageHelpDetail(string key, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Json(new { success = false, message = "Ayar anahtarı boş olamaz." }, JsonRequestBehavior.AllowGet);
            }

            string cleanKey = PageHelpHelper.NormalizeKey(key);
            var predefined = PageHelpHelper.GetPredefinedPages().FirstOrDefault(p => string.Equals(p.Key, cleanKey, StringComparison.OrdinalIgnoreCase));
            var setting = await SettingService.GetSettingObjectByKeyFromDbAsync(cleanKey).ConfigureAwait(false);

            string defaultContent = predefined?.DefaultContent ?? PageHelpHelper.GetDefaultContent(cleanKey);
            bool isCustomized = setting != null && !string.IsNullOrWhiteSpace(setting.SettingValue);
            string content = isCustomized ? setting.SettingValue : defaultContent;
            string pageName = setting?.Name ?? predefined?.PageName ?? cleanKey.Replace(PageHelpHelper.KeySuffix, "");

            return Json(new
            {
                success = true,
                key = cleanKey,
                name = pageName,
                category = predefined?.Category ?? "Özel",
                routeInfo = predefined?.RouteInfo ?? "",
                isCustomized = isCustomized,
                content = content,
                defaultContent = defaultContent,
                updatedDate = setting != null ? setting.UpdatedDate.ToString("dd.MM.yyyy HH:mm") : null
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> SavePageHelp(string key, string name, string content, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Json(new { success = false, message = "Sayfa veya ayar anahtarı boş olamaz." });
            }

            string cleanKey = PageHelpHelper.NormalizeKey(key);
            var setting = await SettingService.GetSettingObjectByKeyFromDbAsync(cleanKey).ConfigureAwait(false);

            string settingTitle = string.IsNullOrWhiteSpace(name) ? cleanKey.Replace(PageHelpHelper.KeySuffix, "") : name.Trim();
            string htmlContent = content?.Trim() ?? string.Empty;

            if (setting == null)
            {
                setting = new EImece.Domain.Entities.Setting
                {
                    Name = settingTitle,
                    SettingKey = cleanKey,
                    Description = DomainConstants.SystemSettings + " - Page Help",
                    SettingValue = htmlContent,
                    IsActive = true,
                    Lang = 1
                };
            }
            else
            {
                setting.Name = settingTitle;
                setting.SettingValue = htmlContent;
                setting.IsActive = true;
            }

            await SettingService.SaveOrEditEntityAsync(setting).ConfigureAwait(false);
            PageHelpHelper.EvictCache(_memoryCacheProvider);
            SettingService.ClearCache();
            _memoryCacheProvider?.ClearAll();

            return Json(new
            {
                success = true,
                message = AdminResource.SuccessfullySavedCompleted,
                key = cleanKey,
                name = settingTitle
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePageHelp(string key, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Json(new { success = false, message = "Ayar anahtarı boş olamaz." });
            }

            string cleanKey = PageHelpHelper.NormalizeKey(key);
            var setting = await SettingService.GetSettingObjectByKeyFromDbAsync(cleanKey).ConfigureAwait(false);

            if (setting != null)
            {
                await SettingService.DeleteEntityAsync(setting).ConfigureAwait(false);
                PageHelpHelper.EvictCache(_memoryCacheProvider);
                SettingService.ClearCache();
                _memoryCacheProvider?.ClearAll();
            }

            string defaultContent = PageHelpHelper.GetDefaultContent(cleanKey);

            return Json(new
            {
                success = true,
                message = "Sayfa yardım bilgisi varsayılana sıfırlandı.",
                key = cleanKey,
                defaultContent = defaultContent
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePageHelp(string key, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Json(new { success = false, message = "Sayfa veya ayar anahtarı boş olamaz." });
            }

            string cleanKey = PageHelpHelper.NormalizeKey(key);
            var predefined = PageHelpHelper.GetPredefinedPages().FirstOrDefault(p => string.Equals(p.Key, cleanKey, StringComparison.OrdinalIgnoreCase));
            var setting = await SettingService.GetSettingObjectByKeyFromDbAsync(cleanKey).ConfigureAwait(false);

            bool isPredefined = predefined != null;

            if (isPredefined)
            {
                string title = predefined.PageName;
                if (setting == null)
                {
                    setting = new EImece.Domain.Entities.Setting
                    {
                        Name = title,
                        SettingKey = cleanKey,
                        Description = DomainConstants.SystemSettings + " - Page Help",
                        SettingValue = string.Empty,
                        IsActive = true,
                        Lang = 1
                    };
                }
                else
                {
                    setting.Name = title;
                    setting.SettingValue = string.Empty;
                    setting.IsActive = true;
                }
                await SettingService.SaveOrEditEntityAsync(setting).ConfigureAwait(false);
            }
            else
            {
                if (setting != null)
                {
                    await SettingService.DeleteEntityAsync(setting).ConfigureAwait(false);
                }
            }

            PageHelpHelper.EvictCache(_memoryCacheProvider);
            SettingService.ClearCache();
            _memoryCacheProvider?.ClearAll();

            return Json(new
            {
                success = true,
                message = "Sayfa yardım rehberi başarıyla kaldırıldı.",
                key = cleanKey,
                isPredefined = isPredefined
            });
        }
    }
}
