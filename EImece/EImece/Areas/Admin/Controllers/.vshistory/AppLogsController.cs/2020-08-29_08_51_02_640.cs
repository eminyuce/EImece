using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using EImece.Domain.Repositories;
using Resources;
using NLog;

namespace EImece.Areas.Admin.Controllers
{
    public class AppLogsController : BaseAdminController
    {
        AppLogRepository AppLogRepository;
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public AppLogsController(AppLogRepository repository)
        {
            this.AppLogRepository  = repository;
        }

        // GET: Admin/AppLogs
        public ActionResult Index(string search = "")
        {
            var logs = AppLogRepository.GetAppLogs(search);
            return View(logs);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
                AppLogRepository.DeleteAppLog(id);
                return ReturnIndexIfNotUrlReferrer("Index");
        } 
        [DeleteAuthorize()]
        public ActionResult RemoveAll(string eventLevel = "" )
        {
           AppLogRepository.RemoveAll(eventLevel);
           return ReturnIndexIfNotUrlReferrer("Index");
        }
    }
}