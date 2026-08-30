using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Filters;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using Microsoft.AspNet.Identity;
using NLog;
using Resources;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

using EImece.Domain.Factories.IFactories;
using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class FaqController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected IFaqService FaqService { get; }
        protected IEntityFactory EntityFactory { get; }

        public FaqController(
            ISettingService settingService,
            IFaqService faqService,
            IEntityFactory entityFactory)
            : base(settingService)
        {
            FaqService = faqService ?? throw new ArgumentNullException(nameof(faqService));
            EntityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
        }

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<Faq, bool>> whereLambda = r => r.Name.Contains(search);
            var result = await FaqService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return View(result);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, String search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search });
            }

            Expression<Func<Faq, bool>> whereLambda = r => r.Name.Contains(search);
            var result = await FaqService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return new QueryableResult<Faq>(result.AsQueryable());
        }

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var item = EntityFactory.GetBaseEntityInstance<Faq>();

            if (id == 0)
            {
            }
            else
            {
                item = await FaqService.GetSingleAsync(id);
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(Faq faq, String saveButton = null)
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
                    await FaqService.SaveOrEditEntityAsync(faq);

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
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.StackTrace);
            }

            RemoveModelState();
            return View(faq);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            Faq Faq = await FaqService.GetSingleAsync(id);
            if (Faq == null)
            {
                return HttpNotFound();
            }
            try
            {
                await FaqService.DeleteEntityAsync(Faq);
                SetSuccessMessage();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete item:" + ex.StackTrace, Faq);
                SetErrorMessage();
                return RedirectToAction("Index");
            }
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync(CancellationToken cancellationToken, string format = "excel")
        {
            return await DownloadFileAsync(format);
        }

        private async Task<ActionResult> DownloadFileAsync(string format = "excel")
        {
            String search = "";
            Expression<Func<Faq, bool>> whereLambda = r => r.Name.Contains(search);
            var Faqs = await FaqService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            var result = from r in Faqs
                         select new
                         {
                             Id = r.Id,
                             Name = r.Name.ToStr(250),
                             Question = r.Question.ToStr(400),
                             Answer = r.Answer.ToStr(30000),
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                         };

            return DownloadFile(result, String.Format("Faqs-{0}", GetCurrentLanguage), format);
        }
    }
}
