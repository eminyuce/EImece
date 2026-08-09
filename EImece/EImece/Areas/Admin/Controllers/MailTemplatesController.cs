using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using NLog;
using Resources;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    [AuthorizeRoles(Domain.Constants.AdministratorRole)]
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
        public ActionResult CreateBackup(int id = 0)
        {
            var item = MailTemplateService.GetSingle(id);
            var itemCopy = JsonConvert.DeserializeObject<MailTemplate>(JsonConvert.SerializeObject(item));
            itemCopy.Name += "-BACKUP";
            itemCopy.Id = 0;
            itemCopy.Body = item.Body;
            MailTemplateService.SaveOrEditEntity(itemCopy);
            return RedirectToAction("Index");
        }
        public ActionResult GenerateHtmlBody(int id = 0)
        {
            if (id <= 0)
            {
                SetErrorMessage("Geçersiz e-posta şablonu.");
                return RedirectToAction("Index");
            }

            var rssTemplate = MailTemplateService.GetSingle(id);
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
                // Rendering can fail for HTML-encoded / incomplete Razor — still offer a download.
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
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(warning))
            {
                // Prefix an HTML comment so the file still opens, and surface a soft status on next page if needed.
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

            return File(fileBytes, "text/html", fileName);
        }

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
                        return RedirectToAction("Index");
                    }
                    else if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
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
                SetSuccessMessage();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete item:" + ex.StackTrace, MailTemplate);
                SetErrorMessage();
                return RedirectToAction("Index");
            }
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync(string format = "excel")
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
    }
}