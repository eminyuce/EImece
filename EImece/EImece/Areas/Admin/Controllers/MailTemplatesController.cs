using EImece.Web.Areas.Admin.Controllers;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Web.Filters;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using NLog;
using Resources;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

using EImece.Domain.Factories.IFactories;

namespace EImece.Areas.Admin.Controllers
{
    [AuthorizeRoles(Domain.Constants.AdministratorRole)]
    public class MailTemplatesController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string IndexAction = "Index";

        protected IMailTemplateService MailTemplateService { get; }
        protected IMailTemplateTestService MailTemplateTestService { get; }
        protected IEntityFactory EntityFactory { get; }
        protected IRazorEngineHelper RazorEngineHelper { get; }

        public MailTemplatesController(
            ISettingService settingService,
            IMailTemplateService mailTemplateService,
            IMailTemplateTestService mailTemplateTestService,
            IEntityFactory entityFactory,
            IRazorEngineHelper razorEngineHelper)
            : base(settingService)
        {
            MailTemplateService = mailTemplateService ?? throw new ArgumentNullException(nameof(mailTemplateService));
            MailTemplateTestService = mailTemplateTestService ?? throw new ArgumentNullException(nameof(mailTemplateTestService));
            EntityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
            RazorEngineHelper = razorEngineHelper ?? throw new ArgumentNullException(nameof(razorEngineHelper));
        }

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<MailTemplate, bool>> whereLambda = r => r.Name.Contains(search);
            var result = await MailTemplateService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return View(result);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, String search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search });
            }

            Expression<Func<MailTemplate, bool>> whereLambda = r => r.Name.Contains(search);
            var result = await MailTemplateService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return new QueryableResult<MailTemplate>(result.AsQueryable());
        }

        public async Task<ActionResult> CreateBackup(CancellationToken cancellationToken, int id = 0)
        {
            var item = await MailTemplateService.GetSingleAsync(id);
            var itemCopy = JsonConvert.DeserializeObject<MailTemplate>(JsonConvert.SerializeObject(item));
            itemCopy.Name += "-BACKUP";
            itemCopy.Id = 0;
            itemCopy.Body = item.Body;
            await MailTemplateService.SaveOrEditEntityAsync(itemCopy);
            return RedirectToAction(IndexAction);
        }

        public async Task<ActionResult> GenerateHtmlBody(CancellationToken cancellationToken, int id = 0)
        {
            if (id <= 0)
            {
                SetErrorMessage("Geçersiz e-posta şablonu.");
                return RedirectToAction(IndexAction);
            }

            var rssTemplate = await MailTemplateService.GetSingleAsync(id);
            if (rssTemplate == null)
            {
                return HttpNotFound();
            }

            string body = null;
            string warning = null;

            try
            {
                body = RazorEngineHelper.GenerateRssEmailTemplate(rssTemplate);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GenerateHtmlBody render failed for MailTemplate Id = {0}", id);
                warning = ex.Message;
                body = System.Net.WebUtility.HtmlDecode(rssTemplate.Body ?? string.Empty);
            }

            if (string.IsNullOrEmpty(body))
            {
                body = rssTemplate.Body ?? string.Empty;
            }

            if (string.IsNullOrEmpty(body))
            {
                SetErrorMessage("HTML gövde oluşturulamadı: şablon içeriği boş.");
                return RedirectToAction(IndexAction);
            }

            if (!string.IsNullOrEmpty(warning))
            {
                body = "<!-- HTML oluşturulurken uyarı: "
                    + System.Net.WebUtility.HtmlEncode(warning)
                    + " — ham şablon içeriği indirildi. -->"
                    + Environment.NewLine
                    + body;
                SetErrorMessage("Şablon derlenemedi; ham HTML içeriği indirildi. Detay: " + warning);
            }

            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(body);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string safeName = string.IsNullOrWhiteSpace(rssTemplate.Name) ? ("MailTemplate-" + id) : rssTemplate.Name;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                safeName = safeName.Replace(c, '_');
            }
            string fileName = string.Format("{0}_{1}.html", safeName, timestamp);

            return File(fileBytes, MediaTypeNames.Text.Html, fileName);
        }

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var item = EntityFactory.GetBaseEntityInstance<MailTemplate>();

            if (id == 0)
            {
            }
            else
            {
                item = await MailTemplateService.GetSingleAsync(id);
            }

            return View(item);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(MailTemplate MailTemplate, String saveButton = null)
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
                    await MailTemplateService.SaveOrEditEntityAsync(MailTemplate);

                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction(IndexAction);
                    }
                    else if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, MailTemplate);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            RemoveModelState();
            return View(MailTemplate);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MailTemplate MailTemplate = await MailTemplateService.GetSingleAsync(id);
            if (MailTemplate == null)
            {
                return HttpNotFound();
            }
            try
            {
                await MailTemplateService.DeleteEntityAsync(MailTemplate);
                SetSuccessMessage();
                return RedirectToAction(IndexAction);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete item:" + ex.StackTrace, MailTemplate);
                SetErrorMessage();
                return RedirectToAction(IndexAction);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<JsonResult> InspectTemplate()
        {
            var request = ReadJsonBody<SendMailTemplateTestRequest>() ?? new SendMailTemplateTestRequest();
            try
            {
                var preview = await MailTemplateTestService.InspectAsync(request, GetDefaultTestRecipient());
                return JsonPayload(new { success = true, data = preview });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "InspectTemplate failed for MailTemplate Id = {0}", request.Id);
                return JsonPayload(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<JsonResult> PreviewTestEmail()
        {
            var request = ReadJsonBody<SendMailTemplateTestRequest>() ?? new SendMailTemplateTestRequest();
            try
            {
                var render = await MailTemplateTestService.PreviewAsync(request);
                if (!render.Success)
                {
                    return JsonPayload(new { success = false, message = render.ErrorMessage });
                }

                return JsonPayload(new { success = true, subject = render.Subject, body = render.Body });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "PreviewTestEmail failed for MailTemplate Id = {0}", request.Id);
                return JsonPayload(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<JsonResult> SendTestEmail()
        {
            var request = ReadJsonBody<SendMailTemplateTestRequest>() ?? new SendMailTemplateTestRequest();
            try
            {
                var result = await MailTemplateTestService.SendTestEmailAsync(request);
                return JsonPayload(new { success = result.Success, message = result.Message, subject = result.Subject });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "SendTestEmail failed for MailTemplate Id = {0}", request.Id);
                return JsonPayload(new { success = false, message = "E-posta gönderilemedi: " + ex.ToFormattedString() });
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
            Expression<Func<MailTemplate, bool>> whereLambda = r => r.Name.Contains(search);
            var mailTemplates = await MailTemplateService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            var result = from r in mailTemplates
                         select new
                         {
                             Id = r.Id,
                             Name = r.Name.ToStr(250),
                             Subject = r.Subject.ToStr(400),
                             Body = r.Body.ToStr(30000),
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                         };

            return DownloadFile(result, String.Format("MailTemplates-{0}", GetCurrentLanguage), format);
        }

        private string GetDefaultTestRecipient()
        {
            return User != null && User.Identity != null
                ? User.Identity.GetUserName()
                : string.Empty;
        }

        private T ReadJsonBody<T>()
        {
            if (Request == null || Request.InputStream == null)
            {
                return default(T);
            }

            if (Request.InputStream.CanSeek)
            {
                Request.InputStream.Position = 0;
            }

            using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8, true, 1024, true))
            {
                var json = reader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(json))
                {
                    return default(T);
                }

                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        private JsonResult JsonPayload(object data)
        {
            var result = Json(data, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue;
            return result;
        }
    }
}
