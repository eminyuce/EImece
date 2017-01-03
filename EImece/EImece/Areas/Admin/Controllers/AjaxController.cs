using EImece.Domain.Entities;
using EImece.Domain.GenericRepositories;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class AjaxController : BaseAdminController
    {

        [HttpPost]
        public ActionResult StoryGridItem(List<String> values)
        {
            StoryService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult StoryMainPageImageGridItem(List<String> values)
        {
            MainPageImageService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult StoryCategoryGridItem(List<String> values)
        {
            StoryCategoryService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }
        // GET: Admin/Ajax
        [HttpPost]
        public ActionResult DeleteSubscriberGridItem(List<String> values)
        {
            SubsciberService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteProductGridItem(List<String> values)
        {
            ProductService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteTemplateGridItem(List<String> values)
        {
            TemplateService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteProductCategoriesGridItem(List<String> values)
        {
            ProductCategoryService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteMediaGridItem(List<String> values)
        {
            FileStorageService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeMainPageImageGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            MainPageImageService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeMediaGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            FileStorageService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeMenusGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            MenuService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeProductCategoriesGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            ProductCategoryService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeProductGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            ProductService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeStoryGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            StoryService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeStoryCategoryGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            StoryCategoryService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeTagGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {

            TagService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeTagCategoriesGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            TagCategoryService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeTemplateGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            TemplateService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetProductTags(EImeceLanguage language, int productId = 0)
        {
            var tags = TagCategoryService.GetTagsByTagType(EImeceTagType.Products, language);
            var productTags = ProductService.GetProductTagsByProductId(productId).Select(r => r.TagId).ToList();
            var tempData = new TempDataDictionary();
            tempData["selectedTags"] = productTags;
            var html = this.RenderPartialToString(
                        @"~/Areas/Admin/Views/Shared/pProductsTag.cshtml",
                        new ViewDataDictionary(tags), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
        }
        //C:\Projects\StoryEngine\_imagesSample\samples2
        public ActionResult GetImageTags(EImeceLanguage language)
        {
            var tags = TagCategoryService.GetTagsByTagType(EImeceTagType.Images, language);
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        @"~/Areas/Admin/Views/Shared/pImagesTag.cshtml",
                        new ViewDataDictionary(tags), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
        }

    }
}