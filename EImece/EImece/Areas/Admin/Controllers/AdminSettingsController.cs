using EImece.Domain.Models.AdminModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class AdminSettingsController : BaseAdminController
    {
        // GET: Admin/AdminSettings
        public ActionResult Index()
        {
            SettingModel r = SettingService.GetSettingModel(CurrentLanguage);
            return View(r);
        }
        [HttpPost]
        public ActionResult Index(SettingModel settingModel)
        {
            SettingService.SaveSettingModel(settingModel);
            return RedirectToAction("Index");
        }
        
    }
}