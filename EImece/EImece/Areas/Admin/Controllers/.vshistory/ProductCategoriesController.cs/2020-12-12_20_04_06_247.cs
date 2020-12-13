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
        public ActionResult Index(String search = "")
        {
            ViewBag.ProductCategoryTree = ProductCategoryService.BuildTree(null, CurrentLanguage);
            var productCategories = ProductCategoryService.GetAdminProductCategories(search, CurrentLanguage);
            ViewBag.ProductCategoryLeaves = ProductCategoryService.GetProductCategoryLeaves(null, CurrentLanguage);
            return View(productCategories);
        }
        [HttpGet]
        public ActionResult MoveProductCategory()
        {
            ViewBag.ProductCategoryDropDownList= GetProductCategoryTreeDropDownList();
            ViewBag.ProductCategoryTree = ProductCategoryService.BuildTree(null, CurrentLanguage);
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoveProductCategory(MoveProductCategory moveProductCategory)
        {
            if (moveProductCategory == null)
            {
                return HttpNotFound();
            }
            if (moveProductCategory.FirstCategoryId > 0 && moveProductCategory.SecondCategoryId > 0)
            {
                var firstCategoryId = ProductCategoryService.GetBaseContent(moveProductCategory.FirstCategoryId);
                var secondCategory = ProductCategoryService.GetBaseContent(moveProductCategory.SecondCategoryId);
                secondCategory.ParentId = firstCategoryId.Id;
                ProductCategoryService.SaveOrEditEntity(secondCategory);
            }
            else if (moveProductCategory.SecondCategoryId > 0)
            {
                var secondCategory = ProductCategoryService.GetBaseContent(moveProductCategory.SecondCategoryId);
                secondCategory.ParentId = 0;
                ProductCategoryService.SaveOrEditEntity(secondCategory);
            }
            return RedirectToAction("MoveProductCategory");
        }
        private List<SelectListItem> GetProductCategoryTreeDropDownList()
        {
            var resultListItem = new List<SelectListItem>();
            resultListItem.Add(new SelectListItem() { Text = AdminResource.MakeItRootCategory, Value = "0" });
            foreach (var item in ProductCategoryService.BuildTree(null, CurrentLanguage))
            {
                resultListItem.Add(new SelectListItem() { Text = item.TextWithArrow, Value = item.ProductCategory.Id.ToStr() });
                GetProductCategoryChildrenTreeDropDownList(resultListItem, item);
            }

            return resultListItem;
        }
        private void GetProductCategoryChildrenTreeDropDownList(List<SelectListItem> resultListItem,  ProductCategoryTreeModel productCategoryTreeModel)
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

        private List<SelectListItem> GetTemplatesDropDown()
        {
            var templates = TemplateService.GetActiveBaseEntities(true, CurrentLanguage);

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
        public ActionResult SaveOrEdit(int id = 0)
        {
            var content = EntityFactory.GetBaseContentInstance<ProductCategory>();
            var parentCategory = EntityFactory.GetBaseContentInstance<ProductCategory>();
            ViewBag.ProductCategoryTree = ProductCategoryService.BuildTree(null, CurrentLanguage);
            ViewBag.ProductCategoryLeaves = ProductCategoryService.GetProductCategoryLeaves(null, CurrentLanguage);
            ViewBag.Templates = GetTemplatesDropDown();
            if (id == 0)
            {
                content.ParentId = 0;
            }
            else
            {
                content = ProductCategoryService.GetBaseContent(id);
                parentCategory = ProductCategoryService.GetSingle(content.ParentId);
                if(content.ParentId > 0 && parentCategory == null)
                {
                    throw new ArgumentException("ParentId "+ content.ParentId+" parent cannot be NULL");
                }
            }
            ViewBag.ParentCategory = parentCategory;
            return View(content);
        }

        //
        // POST: /ProductCategory/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOrEdit(ProductCategory productCategory, HttpPostedFileBase postedImage = null, String saveButton = null)
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
                    ProductCategoryService.SaveOrEditEntity(productCategory);

                    if (!String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return ReturnTempUrl("Index");
                    }else if (!String.IsNullOrEmpty(saveButton) && ModelState.IsValid && saveButton.Equals(AdminResource.SaveButtonText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, productCategory);
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }
            ViewBag.ProductCategoryTree = ProductCategoryService.BuildTree(null, CurrentLanguage);
            ViewBag.ProductCategoryLeaves = ProductCategoryService.GetProductCategoryLeaves(null, CurrentLanguage);
            ViewBag.Templates = GetTemplatesDropDown();
            
            RemoveModelState();
            return View(productCategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductCategory productCategory = ProductCategoryService.GetSingle(id.Value);
            if (productCategory == null)
            {
                return HttpNotFound();
            }

            try
            {
                ProductCategoryService.DeleteProductCategory(productCategory.Id);
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, productCategory);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace);
            }

            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        public async Task<ActionResult> ExportExcel()
        {
            return await Task.Run(() =>
            {
                return DownloadFile();
            }).ConfigureAwait(true);
        }

        private ActionResult DownloadFile()
        {
            String search = "";
            Expression<Func<ProductCategory, bool>> whereLambda = r => string.Equals(r.Name, r.Name, StringComparison.OrdinalIgnoreCase);
            var productCategories = ProductCategoryService.SearchEntities(whereLambda, search, CurrentLanguage);

            var result = from r in productCategories
                         select new
                         {
                             Id = r.Id.ToStr(250),
                             ParentId = r.ParentId.ToStr(250),
                             Name = r.Name.ToStr(250),
                             CreatedDate = r.CreatedDate.ToStr(250),
                             UpdatedDate = r.UpdatedDate.ToStr(250),
                             IsActive = r.IsActive.ToStr(250),
                             Position = r.Position.ToStr(250),
                             Description = r.Description,
                             MainPage = r.MainPage.ToStr(250),
                             ImageState = r.ImageState.ToStr(250),
                             MainImageId = r.MainImageId.ToStr(250)
                         };

            return DownloadFile(result, string.Format("ProductCategories-{0}", GetCurrentLanguage));
        }
    }
}