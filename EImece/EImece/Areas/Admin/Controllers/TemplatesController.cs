using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class TemplatesController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        // GET: Admin/Template
        public ActionResult Index(String search="")
        {
            Expression<Func<Template, bool>> whereLambda = r => r.Name.ToLower().Contains(search.Trim().ToLower());
            var templates = TemplateService.SearchEntities(whereLambda, search);
            return View(templates);
        }
        public ActionResult SaveOrEdit(int id = 0)
        {
            var template = Template.GetInstance<Template>();  

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
        public ActionResult SaveOrEdit(Template template)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    TemplateService.SaveOrEditEntity(template);
                    int contentId = template.Id;
                    return RedirectToAction("Index");
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.Message, template);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator." + ex.Message.ToString());
            }
            return View(template);
        }
        public ActionResult Delete(int id = 0)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Template content = TemplateService.GetSingle(id);
            if (content == null)
            {
                return HttpNotFound();
            }


            return View(content);
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
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(template);

        }
    }
}