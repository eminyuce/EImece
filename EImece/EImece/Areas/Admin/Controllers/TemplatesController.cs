using EImece.Domain.Entities;
using EImece.Domain.Helpers;
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
using System.Xml.Linq;

namespace EImece.Areas.Admin.Controllers
{
    public class TemplatesController : BaseAdminController
    {
        [Inject]
        public XmlEditorHelper XmlEditorHelper { get; set; }

        private const string ProductSpescUrl = "ProductSpescUrl";
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // GET: Admin/Template
        public ActionResult Index(String search = "")
        {
            Expression<Func<Template, bool>> whereLambda = r => r.Name.Contains(search);
            var templates = TemplateService.SearchEntities(whereLambda, search, CurrentLanguage);
            return View(templates);
        }

        public ActionResult SaveOrEdit(int id = 0)
        {
            TempData[ProductSpescUrl] = Request.UrlReferrer.ToStr();
            var template = EntityFactory.GetBaseEntityInstance<Template>();
            ViewBag.XmlEditorConfiguration = XmlEditorHelper.GenerateXmlEditor(id);
            if (id == 0)
            {
            }
            else
            {
                template = TemplateService.GetSingle(id);
            }

            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SaveOrEdit(Template template)
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
                    TemplateService.SaveOrEditEntity(template);
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
                else
                {
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.Message, template);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message.ToString());
            }
            ViewBag.XmlEditorConfiguration = XmlEditorHelper.GenerateXmlEditor();
            return View(template);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Template template = TemplateService.GetSingle(id);
            if (template == null)
            {
                return HttpNotFound();
            }
            try
            {
                TemplateService.DeleteEntity(template);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete template:" + ex.StackTrace, template);
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
            Expression<Func<Template, bool>> whereLambda = r => r.Name.Contains(search);
            var templates = TemplateService.SearchEntities(whereLambda, search, CurrentLanguage);

            var result = from r in templates
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             Name = r.Name.ToStr(250),
                             TemplateXml = r.TemplateXml,
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                         };

            return DownloadFile(result, String.Format("Templates-{0}", GetCurrentLanguage));
        }
    }
}