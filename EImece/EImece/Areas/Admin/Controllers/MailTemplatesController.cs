using EImece.Domain.Entities;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.AdminModels;
using Microsoft.AspNet.Identity;
using NLog;
using System;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class MailTemplatesController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index(String search = "")
        {
            Expression<Func<MailTemplate, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var result = MailTemplateService.SearchEntities(whereLambda, search, CurrentLanguage);
            return View(result);
        }

        //
        // GET: /MailTemplate/Create

        public ActionResult SaveOrEdit(int id = 0)
        {
            var item = EntityFactory.GetBaseEntityInstance<MailTemplate>();

            if (id == 0)
            {
            }
            else
            {
                item = MailTemplateService.GetSingle(id);
            }

            ViewBag.RazorRenderResultBody = RazorEngineHelper.GetRenderOutput(item.Body); ;
            ViewBag.RazorRenderResultSubject = RazorEngineHelper.GetRenderOutput(item.Subject);
            return View(item);
        }

        //
        // POST: /MailTemplate/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(MailTemplate MailTemplate)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (MailTemplate.Id == 0)
                    {
                        MailTemplate.AddUserId = User.Identity.GetUserName();
                        MailTemplate.UpdateUserId = User.Identity.GetUserName();
                    }
                    else
                    {
                        MailTemplate.UpdateUserId = User.Identity.GetUserName();
                    }

                    MailTemplate.Lang = CurrentLanguage;
                    MailTemplateService.SaveOrEditEntity(MailTemplate);
                    int itemId = MailTemplate.Id;
                    return RedirectToAction("Index");
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, MailTemplate);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            RazorRenderResult result = RazorEngineHelper.GetRenderOutput(MailTemplate.Body);
            ViewBag.RazorRenderResult = result;
            return View(MailTemplate);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            MailTemplate MailTemplate = MailTemplateService.GetSingle(id);
            if (MailTemplate == null)
            {
                return HttpNotFound();
            }
            try
            {
                MailTemplateService.DeleteEntity(MailTemplate);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete item:" + ex.StackTrace, MailTemplate);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(MailTemplate);
        }
    }
}