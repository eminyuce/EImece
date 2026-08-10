using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.AdminModels;
using NLog;
using Resources;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

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

        public ActionResult BackUpDb()
        {
            BackupService backupService = new BackupService("");
            backupService.BackupSystemDatabase();

            return Content(@"SUCCESSFULLY BACK UP DB: C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS\MSSQL\Backup\");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SystemSettings(CancellationToken cancellationToken, SystemSettingModel settingModel)
        {
            await SettingService.SaveSystemSettingModelAsync(settingModel);
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
    }
}
