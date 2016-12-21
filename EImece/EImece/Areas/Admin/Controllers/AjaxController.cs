using EImece.Domain.Entities;
using EImece.Domain.GenericRepositories;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using System;
using System.Collections.Generic;
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
            BaseEntityRepository.DeleteBaseEntity(StoryRepository, values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult StoryCategoryGridItem(List<String> values)
        {
            BaseEntityRepository.DeleteBaseEntity(StoryCategoryRepository, values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }
        // GET: Admin/Ajax
        [HttpPost]
        public ActionResult DeleteSubscriberGridItem(List<String> values)
        {
            BaseEntityRepository.DeleteBaseEntity(SubscriberRepository, values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteProductGridItem(List<String> values)
        {
            BaseEntityRepository.DeleteBaseEntity(ProductRepository, values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeProductGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            BaseEntityRepository.ChangeGridBaseEntityOrderingOrState(ProductRepository, values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeTagGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            BaseEntityRepository.ChangeGridBaseEntityOrderingOrState(TagRepository, values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangeTagCategoriesGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            BaseEntityRepository.ChangeGridBaseEntityOrderingOrState(TagCategoryRepository, values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetProductTags(EImeceLanguage language, int productId = 0)
        {
            var tags = TagCategoryRepository.GetTagsByTagType(EImeceTagType.Products, language);
            var productTags = ProductTagRepository.GetAllByProductId(productId).Select(r => r.TagId).ToList();
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
            var tags = TagCategoryRepository.GetTagsByTagType(EImeceTagType.Images, language);
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        @"~/Areas/Admin/Views/Shared/pImagesTag.cshtml",
                        new ViewDataDictionary(tags), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFiles_ForEntry(int baseid)
        {
            //var files = BlogsRepository.GetBlogFiles(baseid);

            //var results = files.Select(x => PartialViewToString.RenderPartialToString(
            //    this.ControllerContext,
            //    "DisplayTemplates/FileManager/NwmBlogFile",
            //    new ViewDataDictionary(x),
            //    new TempDataDictionary()
            //));

            //return Json(results, JsonRequestBehavior.AllowGet);

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddFile_ForEntry(int baseid, FileStorage data)
        {
            //int storyid = baseid.DecodeBase32_Int();
            //data.BlogPostId = baseid;


            //var num = BlogsRepository.SetBlogFile(data);

            //data.BlogFileId = num;
            ////call the repo to add the file to the story

            ////return the data used
            ///*return Json(new
            //{
            //    BlogFileId = data.BlogFileId,
            //    FileNameUrl = data.FileNameUrl,
            //    IsWebImage = data.IsWebImage,
            //    Caption = "",
            //    Width = data.Width,
            //    Height = data.Height
            //});*/

            //var html = PartialViewToString.RenderPartialToString(
            //    this.ControllerContext,
            //    "DisplayTemplates/FileManager/NwmBlogFile",
            //    new ViewDataDictionary(data),
            //    new TempDataDictionary()
            //);

            return Json("");
        }

        public ActionResult RemoveFile_ForEntry(int targetid, string baseid)
        {
            // int storyid = baseid.DecodeBase32_Int();

            //call the repo to remove the file from the story
            //BlogsRepository.SetBlogFileActive(targetid, false);

            //return a blank, don't have anything to send right now
            //don't have to unless a custom callback requires some kind of data
            return Json("");
        }

        public ActionResult ReorderFile_ForEntry(int targetid, int baseid, bool movingUp)
        {
            return Json("");
        }
    }
}