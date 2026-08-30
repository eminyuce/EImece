using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.DependencyInjection;
using EImece.Filters;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;
using ListEntity = EImece.Domain.Entities.List;

using EImece.Domain.Factories.IFactories;
using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    [AuthorizeRoles(Domain.Constants.AdministratorRole)]
    public class TemplatesController : BaseAdminController
    {
        protected ITemplateService TemplateService { get; }
        protected IListService ListService { get; }
        protected IEntityFactory EntityFactory { get; }
        protected XmlEditorHelper XmlEditorHelper { get; }

        private const string ProductSpescUrl = "ProductSpescUrl";
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TemplatesController(
            ISettingService settingService,
            ITemplateService templateService,
            IListService listService,
            IEntityFactory entityFactory,
            XmlEditorHelper xmlEditorHelper)
            : base(settingService)
        {
            TemplateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            ListService = listService ?? throw new ArgumentNullException(nameof(listService));
            EntityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
            XmlEditorHelper = xmlEditorHelper ?? throw new ArgumentNullException(nameof(xmlEditorHelper));
        }

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<Template, bool>> whereLambda = r => r.Name.Contains(search);
            var templates = await TemplateService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return View(templates);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, String search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search });
            }

            Expression<Func<Template, bool>> whereLambda = r => r.Name.Contains(search);
            var templates = await TemplateService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return new QueryableResult<Template>(templates.AsQueryable());
        }

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            TempData[ProductSpescUrl] = Request.UrlReferrer.ToStr();
            var template = EntityFactory.GetBaseEntityInstance<Template>();
            ViewBag.XmlEditorConfiguration = XmlEditorHelper.GenerateXmlEditor(id);
            PopulateTemplateBuilderListNames();
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
        public async Task<ActionResult> SaveOrEdit(Template template, String saveButton = null)
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
                            ViewBag.XmlEditorConfiguration = XmlEditorHelper.GenerateXmlEditor();
                            PopulateTemplateBuilderListNames();
                            return View(template);
                        }
                    }

                    template.Lang = CurrentLanguage;
                    await TemplateService.SaveOrEditEntityAsync(template);

                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (string.IsNullOrEmpty(TempData[ProductSpescUrl].ToStr()))
                        {
                            return RedirectToAction("Index");
                        }
                        return Redirect(TempData[ProductSpescUrl].ToStr());
                    }

                    ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                    RemoveModelState();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.Message, template);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message.ToString());
            }
            ViewBag.XmlEditorConfiguration = XmlEditorHelper.GenerateXmlEditor();
            PopulateTemplateBuilderListNames();
            return View(template);
        }

        private void PopulateTemplateBuilderListNames()
        {
            try
            {
                var lists = ListService.GetListItems() ?? Enumerable.Empty<ListEntity>();
                ViewBag.ListNames = lists
                    .Where(r => r != null && r.IsActive && r.IsValues && !string.IsNullOrWhiteSpace(r.Name))
                    .OrderBy(r => r.Position)
                    .ThenBy(r => r.Name)
                    .Select(r => r.Name)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Unable to load list names for template builder");
                ViewBag.ListNames = new List<string>();
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize]

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
