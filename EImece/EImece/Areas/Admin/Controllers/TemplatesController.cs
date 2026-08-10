using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Helpers.AttributeHelper;
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
using System.Xml.Linq;

namespace EImece.Areas.Admin.Controllers
{
    [AuthorizeRoles(Domain.Constants.AdministratorRole)]
    public class TemplatesController : BaseAdminController
    {
        [Inject]
        public XmlEditorHelper XmlEditorHelper { get; set; }

        private const string ProductSpescUrl = "ProductSpescUrl";
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<Template, bool>> whereLambda = r => r.Name.Contains(search);
            var templates = await TemplateService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return View(templates);
        }

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            TempData[ProductSpescUrl] = Request.UrlReferrer.ToStr();
            var template = EntityFactory.GetBaseEntityInstance<Template>();
            ViewBag.XmlEditorConfiguration = XmlEditorHelper.GenerateXmlEditor(id);
            if (id == 0)
            {
            }
            else
            {
                template = await TemplateService.GetSingleAsync(id);
            }

            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> SaveOrEdit(Template template)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!String.IsNullOrEmpty(template.TemplateXml))
                    {
                        try
                        {
                            XDocument xdoc = XDocument.Parse(template.TemplateXml);
                            var groups = xdoc.Root.Descendants("group");
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("TemplateXml", "XDocument format exception while parsing it:" + ex.Message);
                            return View(template);
                        }
                    }

                    template.Lang = CurrentLanguage;
                    await TemplateService.SaveOrEditEntityAsync(template);
                    int contentId = template.Id;
                    if (string.IsNullOrEmpty(TempData[ProductSpescUrl].ToStr()))
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return Redirect(TempData[ProductSpescUrl].ToStr());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.Message, template);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message.ToString());
            }
            ViewBag.XmlEditorConfiguration = XmlEditorHelper.GenerateXmlEditor();
            return View(template);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            Template template = await TemplateService.GetSingleAsync(id);
            if (template == null)
            {
                return HttpNotFound();
            }
            try
            {
                await TemplateService.DeleteEntityAsync(template);
                SetSuccessMessage();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete template:" + ex.StackTrace, template);
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
            Expression<Func<Template, bool>> whereLambda = r => r.Name.Contains(search);
            var templates = await TemplateService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);

            var result = from r in templates
                         select new
                         {
                             Id = r.Id,
                             Name = r.Name.ToStr(250),
                             TemplateXml = r.TemplateXml,
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                         };

            return DownloadFile(result, String.Format("Templates-{0}", GetCurrentLanguage), format);
        }
    }
}
