﻿using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.AdminModels;
using NLog;
using Resources;
using System;
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
            SqlConnection con = new SqlConnection();
            SqlCommand sqlcmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();

            con.ConnectionString = ConfigurationManager.ConnectionStrings["MyConString"].ConnectionString;
            string backupDIR = "~/BackupDB";
            string path = Server.MapPath(backupDIR);

            try
            {
                var databaseName = "MyFirstDatabase";
                con.Open();
                string saveFileName = "HiteshBackup";
                sqlcmd = new SqlCommand("backup database" + databaseName.BKSDatabaseName + "to disk='" + path + "\\" + saveFileName + ".Bak'", con);
                sqlcmd.ExecuteNonQuery();
                con.Close();


                ViewBag.Success = "Backup database successfully";
                return View("Create");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error Occured During DB backup process !<br>" + ex.ToString();
                return View("Create");
            }
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