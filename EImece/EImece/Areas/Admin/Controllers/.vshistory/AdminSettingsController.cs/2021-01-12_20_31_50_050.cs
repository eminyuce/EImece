using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.AdminModels;
using NLog;
using Resources;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class AdminSettingsController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // GET: Admin/AdminSettings
        public ActionResult Index()
        {
            SettingModel r = SettingService.GetSettingModel(CurrentLanguage);
            return View(r);
        }

        [HttpPost]
        public ActionResult Index(SettingModel settingModel)
        {
            SettingService.SaveSettingModel(settingModel, CurrentLanguage);
            ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            return View(SettingService.GetSettingModel(CurrentLanguage));
        }
        public ActionResult BackUpDb()
        {
            BackupService backupService = new BackupService("~/App_Data");
            backupService.BackupAllUserDatabases();
            return View();
        }
        public ActionResult SystemSettings()
        {
            SystemSettingModel r = SettingService.GetSystemSettingModel();
            return View(r);
        }

        [HttpPost]
        public ActionResult SystemSettings(SystemSettingModel settingModel)
        {
            SettingService.SaveSystemSettingModel(settingModel);
            ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            return View(SettingService.GetSystemSettingModel());
        }

        public ActionResult SendSampleEmail()
        {
            String companyName = "Testing company Name";
            var webSiteCompanyEmailAddress = SettingService.GetSettingByKey(Constants.WebSiteCompanyEmailAddress);
            if (string.IsNullOrEmpty(webSiteCompanyEmailAddress))
            {
                ModelState.AddModelError("", AdminResource.WebSiteCompanyEmailAddressRequired);
                return View("SystemSettings", SettingService.GetSystemSettingModel());
            }
            var emailAccount = SettingService.GetEmailAccount();
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

            return View("SystemSettings", SettingService.GetSystemSettingModel());
        }
    }
}