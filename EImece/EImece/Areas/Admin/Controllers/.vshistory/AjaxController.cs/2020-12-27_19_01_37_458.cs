using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Repositories;
using Ninject;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    public class AjaxController : BaseAdminController
    {
        [Inject]
        public ApplicationDbContext ApplicationDbContext { get; set; }

        private AppLogRepository AppLogRepository { get; set; }

        public AjaxController(AppLogRepository AppLogRepository)
        {
            this.AppLogRepository = AppLogRepository;
        }

        [HttpGet]
        public async Task<JsonResult> SearchAutoComplete(String term, String action, String controller)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(term) || string.IsNullOrEmpty(action) || string.IsNullOrEmpty(controller))
                {
                    throw new ArgumentException("term or action or controller cannot be empty");
                }
                String searchKey = term.ToStr().ToLower(CultureInfo.InvariantCulture).Trim();
                var list = new List<String>();
                Boolean isIndexAction = action.Equals("Index", StringComparison.InvariantCultureIgnoreCase);
                if (isIndexAction && controller.Equals("Products", StringComparison.InvariantCultureIgnoreCase))
                {
                    Expression<Func<Product, bool>> whereLambda1 = r => r.Name.ToLower().Contains(searchKey);
                    list = ProductService.SearchEntities(whereLambda1, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
                }
                else if (isIndexAction && controller.Equals("Stories", StringComparison.InvariantCultureIgnoreCase))
                {
                    Expression<Func<Story, bool>> whereLambda1 = r => r.Name.ToLower().Contains(searchKey);
                    list = StoryService.SearchEntities(whereLambda1, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
                }
                else if (isIndexAction && controller.Equals("ProductCategories", StringComparison.InvariantCultureIgnoreCase))
                {
                    Expression<Func<ProductCategory, bool>> whereLambda1 = r => r.Name.ToLower().Contains(searchKey);
                    list = ProductCategoryService.SearchEntities(whereLambda1, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
                }
                else if (isIndexAction && controller.Equals("StoryCategories", StringComparison.InvariantCultureIgnoreCase))
                {
                    Expression<Func<StoryCategory, bool>> whereLambda3 = r => r.Name.ToLower().Contains(searchKey);
                    list = StoryCategoryService.SearchEntities(whereLambda3, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
                }
                else if (isIndexAction &&
                      controller.Equals("Menus", StringComparison.InvariantCultureIgnoreCase))
                {
                    Expression<Func<Menu, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                    list = MenuService.SearchEntities(whereLamba5, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
                }
                else if (isIndexAction &&
                    controller.Equals("Tags", StringComparison.InvariantCultureIgnoreCase))
                {
                    Expression<Func<Tag, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                    list = TagService.SearchEntities(whereLamba5, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
                }
                else if (isIndexAction &&
                   controller.Equals("TagCategories", StringComparison.InvariantCultureIgnoreCase))
                {
                    Expression<Func<TagCategory, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                    list = TagCategoryService.SearchEntities(whereLamba5, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
                }
                else if (isIndexAction &&
                controller.Equals("Subscribers", StringComparison.InvariantCultureIgnoreCase))
                {
                    Expression<Func<Subscriber, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                    list = SubscriberService.SearchEntities(whereLamba5, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
                }
                else if (isIndexAction &&
             controller.Equals("Settings", StringComparison.InvariantCultureIgnoreCase))
                {
                    Expression<Func<Setting, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                    list = SettingService.SearchEntities(whereLamba5, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
                }
                else if (isIndexAction &&
            controller.Equals("MainPageImages", StringComparison.InvariantCultureIgnoreCase))
                {
                    Expression<Func<MainPageImage, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                    list = MainPageImageService.SearchEntities(whereLamba5, searchKey, CurrentLanguage).Select(r => r.Name).ToList();
                }
                else if (isIndexAction &&
        controller.Equals("Users", StringComparison.InvariantCultureIgnoreCase))
                {
                    var users = ApplicationDbContext.Users.AsQueryable();
                    list = users.Where(r => r.Email.ToLower().Contains(searchKey) || r.FirstName.ToLower().Contains(searchKey) || r.LastName.ToLower().Contains(searchKey)).Select(r => r.Email).ToList();
                }

                return Json(list.Take(15).ToList(), JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [DeleteAuthorize()]
        public JsonResult ChangedOrderStatus(int orderId, string orderStatus)
        {
            EImeceOrderStatus? orderStatusEnum = EnumHelper.Parse<EImeceOrderStatus>(orderStatus);
            var order = OrderService.GetSingle(orderId);
            order.OrderStatus = (int)orderStatusEnum.Value;
            OrderService.SaveOrEditEntity(order);
            return Json(Resources.Resource.SuccessfullySavedCompleted, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteTagCategoriesGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                TagCategoryService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteStoryCategoryGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                StoryCategoryService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteProductCommentGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                ProductCommentService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteSettingGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                SettingService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteStoryGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                StoryService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> MainPageImageGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                MainPageImageService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> StoryCategoryGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                StoryCategoryService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        // GET: Admin/Ajax
        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteSubscriberGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                SubscriberService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteAppLogGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                AppLogRepository.DeleteAppLogs(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteProductGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                ProductService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteTemplateGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                TemplateService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteTagGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                TagService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteProductCategoriesGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                ProductCategoryService.DeleteProductCategories(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteMainPageImageGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                MainPageImageService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteFaqGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                FaqService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteMenusGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                MenuService.DeleteMenus(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteMediaGridItem(List<String> values)
        {
            return await Task.Run(() =>
            {
                FileStorageService.DeleteBaseEntity(values);
                return Json(values, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeMainPageImageGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                MainPageImageService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeFaqGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                FaqService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeSubscriberGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                SubscriberService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeProductCommentGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                ProductCommentService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeMediaGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                FileStorageService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeMailTemplateGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                MailTemplateService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeMenusGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                MenuService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeProductCategoriesGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                ProductCategoryService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeProductGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                ProductService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeStoryGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                StoryService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeStoryCategoryGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                StoryCategoryService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeBrandGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                BrandService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeTagGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                TagService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeTagCategoriesGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                TagCategoryService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeTemplateGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            return await Task.Run(() =>
            {
                TemplateService.ChangeGridBaseEntityOrderingOrState(values, checkbox);
                return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> GetProductTags(EImeceLanguage language, int productId = 0)
        {
            return await Task.Run(() =>
            {
                var tags = TagCategoryService.GetTagsByTagType(language);
                if (tags.IsEmpty())
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tags = EntityFilterHelper.FilterTagCategories(tags);
                    var productTags = ProductService.GetProductTagsByProductId(productId).Select(r => r.TagId).ToList();
                    var tempData = new TempDataDictionary();
                    tempData["selectedTags"] = productTags;
                    var html = this.RenderPartialToString(
                                @"~/Areas/Admin/Views/Shared/pSelectedTags.cshtml",
                                new ViewDataDictionary(tags), tempData);
                    return Json(html, JsonRequestBehavior.AllowGet);
                }
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> GetStoryTags(EImeceLanguage language, int storyId = 0)
        {
            return await Task.Run(() =>
            {
                var tags = TagCategoryService.GetTagsByTagType(language);
                if (tags.IsEmpty())
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    tags = EntityFilterHelper.FilterTagCategories(tags);
                    var storyTags = StoryService.GetStoryTagsByStoryId(storyId).Select(r => r.TagId).ToList();
                    var tempData = new TempDataDictionary();
                    tempData["selectedTags"] = storyTags;
                    var html = this.RenderPartialToString(
                                @"~/Areas/Admin/Views/Shared/pSelectedTags.cshtml",
                                new ViewDataDictionary(tags), tempData);
                    return Json(html, JsonRequestBehavior.AllowGet);
                }
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> GetProductDetailToolTip(int productId = 0)
        {
            return await Task.Run(() =>
            {
                var product = ProductService.GetProductDetailViewModelById(productId);
                var html = this.RenderPartialToString(
                            @"~/Areas/Admin/Views/Shared/pProductDetailToolTip.cshtml",
                            new ViewDataDictionary(product), null);
                return Json(html, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<JsonResult> GetTags(EImeceLanguage language)
        {
            return await Task.Run(() =>
            {
                var tags = TagCategoryService.GetTagsByTagType(language);
                var tempData = new TempDataDictionary();
                var html = this.RenderPartialToString(
                            @"~/Areas/Admin/Views/Shared/pImagesTag.cshtml",
                            new ViewDataDictionary(tags), tempData);
                return Json(html, JsonRequestBehavior.AllowGet);
            }).ConfigureAwait(true);
        }
    }
}