using EImece.Domain;
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
            SettingModel r = SettingService.GetSettingModel();
            return View(r);
        }
        
        [HttpPost]
        public ActionResult Index(SettingModel settingModel)
        {
            SettingService.SaveSettingModel(settingModel);
            ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            return View(SettingService.GetSettingModel());
        }

        public ActionResult SendSampleEmail()
        {
            String companyName = "Testing company Name";
            var webSiteCompanyEmailAddress = SettingService.GetSettingByKey(Constants.WebSiteCompanyEmailAddress);
            if (string.IsNullOrEmpty(webSiteCompanyEmailAddress))
            {
                ModelState.AddModelError("", AdminResource.WebSiteCompanyEmailAddressRequired);
                return View("Index", SettingService.GetSettingModel());
            }
            var emailAccount = SettingService.GetEmailAccount();
            var info = String.Format("From-->{0} {1} To: {2}", webSiteCompanyEmailAddress, companyName, emailAccount.ToString());
            try
            {
                EmailSender.SendEmail(emailAccount,
                  subject:"Test Subject",
                  body:"Test Email Body",
                  fromAddress: emailAccount.Email,
                  fromName: emailAccount.Username,
                  toAddress: webSiteCompanyEmailAddress,
                  toName: companyName);
               
                ModelState.AddModelError("",   AdminResource.SuccessfullySavedCompleted);
            }
            catch (Exception ex)
            {
                Logger.Debug("It could not sent sample Email:" + info);
                ModelState.AddModelError("",   ex.ToFormattedString());
            }

            return View("Index",SettingService.GetSettingModel());
        }
    }
}