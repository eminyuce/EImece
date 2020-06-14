using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class AjaxController : BaseAdminController
    {
        public ActionResult SearchAutoComplete(String term, String action, String controller)
        {
            String searchKey = term.ToStr().ToLower().Trim();
            var list = new List<String>();

            if (action.Equals("Index", StringComparison.InvariantCultureIgnoreCase) &&
                    controller.Equals("Products", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Product, bool>> whereLambda1 = r => r.Name.ToLower().Contains(searchKey);
                list = ProductService.SearchEntities(whereLambda1, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
            }
            else if (action.Equals("Index", StringComparison.InvariantCultureIgnoreCase) &&
                   controller.Equals("Stories", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Story, bool>> whereLambda1 = r => r.Name.ToLower().Contains(searchKey);
                list = StoryService.SearchEntities(whereLambda1, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
            }
            else if (action.Equals("Index", StringComparison.InvariantCultureIgnoreCase) &&
                   controller.Equals("ProductCategories", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<ProductCategory, bool>> whereLambda1 = r => r.Name.ToLower().Contains(searchKey);
                list = ProductCategoryService.SearchEntities(whereLambda1, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
            }
            else if (action.Equals("Index", StringComparison.InvariantCultureIgnoreCase) &&
                   controller.Equals("StoryCategories", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<StoryCategory, bool>> whereLambda3 = r => r.Name.ToLower().Contains(searchKey);
                list = StoryCategoryService.SearchEntities(whereLambda3, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
            }
            else if (action.Equals("Index", StringComparison.InvariantCultureIgnoreCase) &&
                  controller.Equals("Menus", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Menu, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                list = MenuService.SearchEntities(whereLamba5, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
            }
            else if (action.Equals("Index", StringComparison.InvariantCultureIgnoreCase) &&
                controller.Equals("Tags", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Tag, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                list = TagService.SearchEntities(whereLamba5, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
            }
            else if (action.Equals("Index", StringComparison.InvariantCultureIgnoreCase) &&
               controller.Equals("TagCategories", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<TagCategory, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                list = TagCategoryService.SearchEntities(whereLamba5, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
            }
            else if (action.Equals("Index", StringComparison.InvariantCultureIgnoreCase) &&
            controller.Equals("Subscribers", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Subscriber, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                list = SubscriberService.SearchEntities(whereLamba5, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
            }
            else if (action.Equals("Index", StringComparison.InvariantCultureIgnoreCase) &&
         controller.Equals("Settings", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Setting, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                list = SettingService.SearchEntities(whereLamba5, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
            }
            else if (action.Equals("Index", StringComparison.InvariantCultureIgnoreCase) &&
        controller.Equals("MainPageImages", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<MainPageImage, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                list = MainPageImageService.SearchEntities(whereLamba5, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
            }
            else if (action.Equals("Index", StringComparison.InvariantCultureIgnoreCase) &&
    controller.Equals("Users", StringComparison.InvariantCultureIgnoreCase))
            {
                var users = ApplicationDbContext.Users.AsQueryable();
                list = users.Where(r => r.Email.ToLower().Contains(searchKey) || r.FirstName.ToLower().Contains(searchKey) || r.LastName.ToLower().Contains(searchKey)).Select(r => r.Email).ToList();
            }

            return Json(list.Take(15).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public ActionResult DeleteTagCategoriesGridItem(List<String> values)
        {
            TagCategoryService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public ActionResult DeleteStoryCategoryGridItem(List<String> values)
        {
            StoryCategoryService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public ActionResult DeleteSettingGridItem(List<String> values)
        {
            SettingService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public ActionResult DeleteStoryGridItem(List<String> values)
        {
            StoryService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public ActionResult MainPageImageGridItem(List<String> values)
        {
            MainPageImageService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public ActionResult StoryCategoryGridItem(List<String> values)
        {
            StoryCategoryService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/Ajax
        [HttpPost]
        [DeleteAuthorize()]
        public ActionResult DeleteSubscriberGridItem(List<String> values)
        {
            SubscriberService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public ActionResult DeleteProductGridItem(List<String> values)
        {
            ProductService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public ActionResult DeleteTemplateGridItem(List<String> values)
        {
            TemplateService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public ActionResult DeleteTagGridItem(List<String> values)
        {
            TagService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public ActionResult DeleteProductCategoriesGridItem(List<String> values)
        {
            ProductCategoryService.DeleteProductCategories(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public ActionResult DeleteMainPageImageGridItem(List<String> values)
        {
            MainPageImageService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public ActionResult DeleteMenusGridItem(List<String> values)
        {
            MenuService.DeleteMenus(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
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

        public ActionResult ChangeSubscriberGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            SubscriberService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangeMediaGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            FileStorageService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangeMailTemplateGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            MailTemplateService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
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
            var tags = TagCategoryService.GetTagsByTagType(language);
            var productTags = ProductService.GetProductTagsByProductId(productId).Select(r => r.TagId).ToList();
            var tempData = new TempDataDictionary();
            tempData["selectedTags"] = productTags;
            var html = this.RenderPartialToString(
                        @"~/Areas/Admin/Views/Shared/pSelectedTags.cshtml",
                        new ViewDataDictionary(tags), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStoryTags(EImeceLanguage language, int storyId = 0)
        {
            var tags = TagCategoryService.GetTagsByTagType(language);
            var storyTags = StoryService.GetStoryTagsByStoryId(storyId).Select(r => r.TagId).ToList();
            var tempData = new TempDataDictionary();
            tempData["selectedTags"] = storyTags;
            var html = this.RenderPartialToString(
                        @"~/Areas/Admin/Views/Shared/pSelectedTags.cshtml",
                        new ViewDataDictionary(tags), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProductDetailToolTip(int productId = 0)
        {
            var product = ProductService.GetProductById(productId);
            var html = this.RenderPartialToString(
                        @"~/Areas/Admin/Views/Shared/pProductDetailToolTip.cshtml",
                        new ViewDataDictionary(product), null);
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        //C:\Projects\StoryEngine\_imagesSample\samples2
        public ActionResult GetTags(EImeceLanguage language)
        {
            var tags = TagCategoryService.GetTagsByTagType(language);
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        @"~/Areas/Admin/Views/Shared/pImagesTag.cshtml",
                        new ViewDataDictionary(tags), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
        }
    }
}