using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using Microsoft.AspNet.Identity;
using NLog;
using Resources;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class MailTemplatesController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index(String search = "")
        {
            Expression<Func<MailTemplate, bool>> whereLambda = r => r.Name.Contains(search);
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

           // ViewBag.RazorRenderResultBody = RazorEngineHelper.GetRenderOutput(item.Body); ;
          //  ViewBag.RazorRenderResultSubject = RazorEngineHelper.GetRenderOutput(item.Subject);
            return View(item);
        }

        //
        // POST: /MailTemplate/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(MailTemplate MailTemplate, String saveButton = null)
        {
            try
            {
                if (MailTemplate == null)
                {
                    return HttpNotFound();
                }
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
                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return ReturnTempUrl("Index");
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, MailTemplate);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }
            if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
            {
                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
            }
            RemoveModelState();
            return View(MailTemplate);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
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
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync()
        {
            return await Task.Run(() =>
            {
                return DownloadFile();
            }).ConfigureAwait(true);
        }

        private ActionResult DownloadFile()
        {
            String search = "";
            Expression<Func<MailTemplate, bool>> whereLambda = r => r.Name.Contains(search);
            var mailTemplates = MailTemplateService.SearchEntities(whereLambda, search, CurrentLanguage);
            var result = from r in mailTemplates
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             Name = r.Name.ToStr(250),
                             Subject = r.Subject.ToStr(400),
                             Body = r.Body.ToStr(30000),
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                         };

            return DownloadFile(result, String.Format("MailTemplates-{0}", GetCurrentLanguage));
        }
    }
}