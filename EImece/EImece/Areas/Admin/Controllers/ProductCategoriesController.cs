using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.AdminHelperModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class ProductCategoriesController : BaseAdminController
    {
        // GET: Admin/ProductCategories
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            ViewBag.ProductCategoryTree = await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage);
            var productCategories = await ProductCategoryService.GetAdminProductCategoriesAsync(search, CurrentLanguage, cancellationToken);
            ViewBag.ProductCategoryLeaves = await ProductCategoryService.GetProductCategoryLeavesAsync(null, CurrentLanguage, cancellationToken);
            return View(productCategories);
        }

        [HttpGet]
        public async Task<ActionResult> MoveProductCategory(CancellationToken cancellationToken)
        {
            ViewBag.ProductCategoryDropDownList = await GetProductCategoryTreeDropDownListAsync();
            ViewBag.ProductCategoryTree = await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveProductCategory(CancellationToken cancellationToken, MoveProductCategory moveProductCategory)
        {
            if (moveProductCategory == null)
            {
                return HttpNotFound();
            }
            if (moveProductCategory.FirstCategoryId > 0 && moveProductCategory.SecondCategoryId > 0)
            {
                var firstCategoryId = await ProductCategoryService.GetBaseContentAsync(moveProductCategory.FirstCategoryId, cancellationToken);
                var secondCategory = await ProductCategoryService.GetBaseContentAsync(moveProductCategory.SecondCategoryId, cancellationToken);
                secondCategory.ParentId = firstCategoryId.Id;
                await ProductCategoryService.SaveOrEditEntityAsync(secondCategory);
            }
            else if (moveProductCategory.SecondCategoryId > 0)
            {
                var secondCategory = await ProductCategoryService.GetBaseContentAsync(moveProductCategory.SecondCategoryId, cancellationToken);
                secondCategory.ParentId = 0;
                await ProductCategoryService.SaveOrEditEntityAsync(secondCategory);
            }
            return RedirectToAction("MoveProductCategory");
        }

        private async Task<List<SelectListItem>> GetProductCategoryTreeDropDownListAsync()
        {
            var resultListItem = new List<SelectListItem>();
            resultListItem.Add(new SelectListItem() { Text = AdminResource.MakeItRootCategory, Value = "0" });
            foreach (var item in await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage))
            {
                resultListItem.Add(new SelectListItem() { Text = item.TextWithArrow, Value = item.ProductCategory.Id.ToStr() });
                GetProductCategoryChildrenTreeDropDownList(resultListItem, item);
            }

            return resultListItem;
        }

        private void GetProductCategoryChildrenTreeDropDownList(List<SelectListItem> resultListItem, ProductCategoryTreeModel productCategoryTreeModel)
        {
            if (productCategoryTreeModel.Childrens.IsNotEmpty())
            {
                foreach (var item in productCategoryTreeModel.Childrens)
                {
                    resultListItem.Add(new SelectListItem() { Text = item.TextWithArrow, Value = item.ProductCategory.Id.ToStr() });
                    GetProductCategoryChildrenTreeDropDownList(resultListItem, item);
                }
            }
        }

        private async Task<List<SelectListItem>> GetTemplatesDropDownAsync(CancellationToken cancellationToken)
        {
            var templates = await TemplateService.GetActiveBaseEntitiesAsync(true, CurrentLanguage, cancellationToken);

            var resultListItem = new List<SelectListItem>();
            resultListItem.Add(new SelectListItem() { Text = AdminResource.SelectTemplate, Value = "0" });
            foreach (var item in templates)
            {
                resultListItem.Add(new SelectListItem() { Text = item.Name, Value = item.Id.ToStr() });
            }
            return resultListItem;
        }

        //
        // GET: /ProductCategory/Create
        [HttpGet]
        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var content = EntityFactory.GetBaseContentInstance<ProductCategory>();
            var parentCategory = EntityFactory.GetBaseContentInstance<ProductCategory>();
            ViewBag.ProductCategoryTree = await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage);
            ViewBag.ProductCategoryLeaves = await ProductCategoryService.GetProductCategoryLeavesAsync(null, CurrentLanguage, cancellationToken);
            ViewBag.Templates = await GetTemplatesDropDownAsync(cancellationToken);
            if (id == 0)
            {
                content.ParentId = 0;
            }
            else
            {
                content = await ProductCategoryService.GetBaseContentAsync(id, cancellationToken);
                parentCategory = await ProductCategoryService.GetSingleAsync(content.ParentId);
                if (content.ParentId > 0 && parentCategory == null)
                {
                    throw new ArgumentException("ParentId " + content.ParentId + " parent cannot be NULL");
                }
            }
            ViewBag.ParentCategory = parentCategory;
            return View(content);
        }

        //
        // POST: /ProductCategory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, ProductCategory productCategory, HttpPostedFileBase postedImage = null, String saveButton = null)
        {
            try
            {
                if (productCategory == null)
                {
                    return HttpNotFound();
                }

                if (ModelState.IsValid)
                {
                    FilesHelper.SaveFileFromHttpPostedFileBase(postedImage,
                        productCategory.ImageHeight,
                        productCategory.ImageWidth,
                        EImeceImageType.ProductCategoryMainImage,
                        productCategory);
                    if (!productCategory.TemplateId.HasValue)
                    {
                        productCategory.TemplateId = 0;
                    }
                    productCategory.Lang = CurrentLanguage;
                    await ProductCategoryService.SaveOrEditEntityAsync(productCategory);

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
                    ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, productCategory);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }
            ViewBag.ProductCategoryTree = await ProductCategoryService.BuildTreeAsync(null, CurrentLanguage);
            ViewBag.ProductCategoryLeaves = await ProductCategoryService.GetProductCategoryLeavesAsync(null, CurrentLanguage, cancellationToken);
            ViewBag.Templates = await GetTemplatesDropDownAsync(cancellationToken);

            RemoveModelState();
            return View(productCategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductCategory productCategory = await ProductCategoryService.GetSingleAsync(id.Value);
            if (productCategory == null)
            {
                return HttpNotFound();
            }

            try
            {
                await ProductCategoryService.DeleteProductCategoryAsync(productCategory.Id, cancellationToken);
                SetSuccessMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, productCategory);
                SetErrorMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
        }

        [HttpGet]
        public async Task<ActionResult> ExportExcel(CancellationToken cancellationToken, string format = "excel")
        {
            return await DownloadFileAsync(cancellationToken, format);
        }

        private async Task<ActionResult> DownloadFileAsync(CancellationToken cancellationToken, string format = "excel")
        {
            cancellationToken.ThrowIfCancellationRequested();
            String search = "";
            Expression<Func<ProductCategory, bool>> whereLambda = r => string.Equals(r.Name, r.Name, StringComparison.OrdinalIgnoreCase);
            var productCategories = await ProductCategoryService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);

            var result = from r in productCategories
                         select new
                         {
                             Id = r.Id,
                             ParentId = r.ParentId,
                             Name = r.Name.ToStr(250),
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                             Description = r.Description,
                             MainPage = r.MainPage,
                             ImageState = r.ImageState,
                             MainImageId = r.MainImageId
                         };

            return DownloadFile(result, string.Format("ProductCategories-{0}", GetCurrentLanguage), format);
        }
    }
}
