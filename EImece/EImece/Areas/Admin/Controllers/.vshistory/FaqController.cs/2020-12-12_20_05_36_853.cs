using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Services.IServices;
using Microsoft.AspNet.Identity;
using Ninject;
using NLog;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class FaqController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        public ActionResult Index(String search = "")
        {
            Expression<Func<Faq, bool>> whereLambda = r => r.Name.Contains(search);
            var result = FaqService.SearchEntities(whereLambda, search, CurrentLanguage);
            return View(result);
        }

        //
        // GET: /Faq/Create

        public ActionResult SaveOrEdit(int id = 0)
        {
            var item = EntityFactory.GetBaseEntityInstance<Faq>();

            if (id == 0)
            {
            }
            else
            {
                item = FaqService.GetSingle(id);
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(Faq faq, String saveButton = null)
        {
            if (faq == null)
            {
                return HttpNotFound();
            }
            try
            {
                if (ModelState.IsValid)
                {
                    if (faq.Id == 0)
                    {
                        faq.AddUserId = User.Identity.GetUserName();
                        faq.UpdateUserId = User.Identity.GetUserName();
                    }
                    else
                    {
                        faq.UpdateUserId = User.Identity.GetUserName();
                    }

                    faq.Lang = CurrentLanguage;
                    FaqService.SaveOrEditEntity(faq);

                   
                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Index");
                    }
                    else if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, faq);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.StackTrace);
            }
            
            RemoveModelState();
            return View(faq);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public ActionResult DeleteConfirmed(int id)
        {
            Faq Faq = FaqService.GetSingle(id);
            if (Faq == null)
            {
                return HttpNotFound();
            }
            try
            {
                FaqService.DeleteEntity(Faq);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete item:" + ex.StackTrace, Faq);
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
            Expression<Func<Faq, bool>> whereLambda = r => r.Name.Contains(search);
            var Faqs = FaqService.SearchEntities(whereLambda, search, CurrentLanguage);
            var result = from r in Faqs
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             Name = r.Name.ToStr(250),
                             Question = r.Question.ToStr(400),
                             Answer = r.Answer.ToStr(30000),
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                         };

            return DownloadFile(result, String.Format("Faqs-{0}", GetCurrentLanguage));
        }
    }
}