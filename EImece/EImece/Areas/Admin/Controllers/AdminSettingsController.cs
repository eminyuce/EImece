using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.AdminModels;
using Resources;
using System;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class AdminSettingsController : BaseAdminController
    {
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
            try
            {
                String companyname = "Testing Email"; 
                var webSiteCompanyEmailAddress = SettingService.GetSettingByKey(Constants.WebSiteCompanyEmailAddress);
                if (string.IsNullOrEmpty(webSiteCompanyEmailAddress))
                {
                    ModelState.AddModelError("", AdminResource.WebSiteCompanyEmailAddressRequired);
                    return View("Index", SettingService.GetSettingModel());
                }
                EmailSender.SendEmail(SettingService.GetEmailAccount(),
                  "Test Subject",
                  "Test Email Body",
                  webSiteCompanyEmailAddress,
                  companyname,
                  webSiteCompanyEmailAddress,
                  companyname);
                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.ToFormattedString());
            }

            return View("Index",SettingService.GetSettingModel());
        }
    }
}