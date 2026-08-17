using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class TagsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            var result = await TagService.GetAdminPageListAsync(search, CurrentLanguage);
            var isProductPriceEnable = await SettingService.GetSettingObjectByKeyAsync(Constants.IsProductPriceEnable);
            ViewBag.IsProductPriceEnable = isProductPriceEnable;
            return View(result);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, String search = "")
        {
            if (!Request.IsAjaxRequest() && !ControllerContext.IsChildAction)
            {
                return RedirectToAction("Index", new { search });
            }

            var result = await TagService.GetAdminPageListAsync(search, CurrentLanguage);
            var isProductPriceEnable = await SettingService.GetSettingObjectByKeyAsync(Constants.IsProductPriceEnable);
            ViewBag.IsProductPriceEnable = isProductPriceEnable;
            return new QueryableResult<Tag>(result.AsQueryable());
        }

        private async Task<List<SelectListItem>> GetCategoriesSelectListAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<TagCategory> tagCategories = (await TagCategoryService.GetAllAsync()).Where(r => r.IsActive && r.Lang == CurrentLanguage).OrderBy(r => r.Position).ToList();
            return tagCategories.Select(r => new SelectListItem()
            {
                Text = r.Name.ToStr(),
                Value = r.Id.ToStr()
            }).ToList();
        }

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Tag>();
            ViewBag.Categories = await GetCategoriesSelectListAsync(cancellationToken);
            if (id == 0)
            {
            }
            else
            {
                content = await TagService.GetSingleAsync(id);
            }

            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, Tag tag, String saveButton = null)
        {
            try
            {
                if (tag == null)
                {
                    return HttpNotFound();
                }

                if (ModelState.IsValid)
                {
                    tag.Lang = CurrentLanguage;
                    await TagService.SaveOrEditEntityAsync(tag);

                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return RedirectToAction("Index");
                    }

                    ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                }
                else
                {
                    ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, tag);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message);
            }
            RemoveModelState();
            ViewBag.Categories = await GetCategoriesSelectListAsync(cancellationToken);
            return View(tag);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            Tag tag = await TagService.GetSingleAsync(id);
            if (tag == null)
            {
                return HttpNotFound();
            }
            try
            {
                await TagService.DeleteEntityAsync(tag);
                SetSuccessMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, tag);
                SetErrorMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
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
            var tags = await TagService.GetAdminPageListAsync(search, CurrentLanguage);

            var result = from r in tags
                         select new
                         {
                             Id = r.Id,
                             Name = r.Name.ToStr(250),
                             TagCategory = r.TagCategory.Name.ToStr(250),
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                         };

            return DownloadFile(result, String.Format("Tags-{0}", GetCurrentLanguage), format);
        }
    }
}
