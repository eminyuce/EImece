using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Repositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using NLog;

namespace EImece.Areas.Admin.Controllers
{
    public class AjaxController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [Inject]
        public ApplicationDbContext ApplicationDbContext { get; set; }

        private AppLogRepository AppLogRepository { get; set; }

        [Inject]
        public IShoppingCartService ShoppingCartService { get; set; }

        public AjaxController(AppLogRepository AppLogRepository)
        {
            this.AppLogRepository = AppLogRepository;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            try
            {
                var ex = filterContext.Exception;
                var routeData = filterContext.RouteData;
                var action = routeData.Values.ContainsKey("action") ? routeData.Values["action"] : "";
                var controller = routeData.Values.ContainsKey("controller") ? routeData.Values["controller"] : "";
                var user = filterContext.HttpContext?.User?.Identity?.Name ?? "-";
                var url = filterContext.HttpContext?.Request?.Url?.ToString() ?? "-";
                var message = $"Unhandled exception in Admin AjaxController - Controller:{controller} Action:{action} User:{user} Url:{url}";
                Logger.Error(ex, message);
            }
            catch (Exception logEx)
            {
                try
                {
                    // Worst-case fallback to System.Diagnostics if NLog fails
                    System.Diagnostics.Trace.TraceError("Error while logging exception in AjaxController: " + logEx.ToString());
                }
                catch
                {
                }
            }

            base.OnException(filterContext);
        }

        [HttpPost]
        public ActionResult UpdatePrices(UpdatePriceRequest request)
        {
            Logger.Info($"UpdatePrices called by {User?.Identity?.Name ?? "-"} with Percentage={request?.PercentageOfIncreaseOrDecrease}, ProductId={request?.ProductId}, CategoryId={request?.CategoryId}, BrandId={request?.BrandId}, TagId={request?.TagId}");
            try
            {
                if (request == null || request.PercentageOfIncreaseOrDecrease == null)
                {
                    Logger.Warn("UpdatePrices: missing percentage in request");
                    return Json(new { success = false, message = "Yüzde değeri gerekli." }, JsonRequestBehavior.AllowGet);
                }
                var affectedRows = ProductService.UpdatePrices(request);
                Logger.Info($"UpdatePrices completed. AffectedRows={affectedRows}");
                // Başarılı yanıt döndür
                return Json(new { success = true, affectedRows = affectedRows }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "UpdatePrices failed");
                // Hata durumunda yanıt
                return Json(new { success = false, message = $"Hata: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DeleteBaseContentMainImage(int contentId, int imageId, String contentClass)
        {
            Logger.Info($"DeleteBaseContentMainImage called by {User?.Identity?.Name ?? "-"} ContentId={contentId} ImageId={imageId} ContentClass={contentClass}");
            if (string.IsNullOrEmpty(contentClass))
            {
                Logger.Warn("DeleteBaseContentMainImage: contentClass is empty");
                return Json("Error contentClassName does not exists", JsonRequestBehavior.AllowGet);
            }

            if (contentClass.Equals(typeof(Product).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                FileStorageService.DeleteFileStorage(imageId);
                var item = ProductService.GetSingle(contentId);
                item.MainImageId = null;
                ProductService.SaveOrEditEntity(item);
            }
            else if (contentClass.Equals(typeof(Menu).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                FileStorageService.DeleteFileStorage(imageId);
                var item = MenuService.GetSingle(contentId);
                item.MainImageId = null;
                MenuService.SaveOrEditEntity(item);
            }
            else if (contentClass.Equals(typeof(ProductCategory).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                FileStorageService.DeleteFileStorage(imageId);
                var item = ProductCategoryService.GetSingle(contentId);
                item.MainImageId = null;
                ProductCategoryService.SaveOrEditEntity(item);
            }
            else if (contentClass.Equals(typeof(Story).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                FileStorageService.DeleteFileStorage(imageId);
                var item = StoryService.GetSingle(contentId);
                item.MainImageId = null;
                StoryService.SaveOrEditEntity(item);
            }
            else if (contentClass.Equals(typeof(StoryCategory).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                FileStorageService.DeleteFileStorage(imageId);
                var item = StoryCategoryService.GetSingle(contentId);
                item.MainImageId = null;
                StoryCategoryService.SaveOrEditEntity(item);
            }
            else if (contentClass.Equals(typeof(MainPageImage).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                FileStorageService.DeleteFileStorage(imageId);
                var item = MainPageImageService.GetSingle(contentId);
                item.MainImageId = null;
                MainPageImageService.SaveOrEditEntity(item);
            }
            else
            {
                throw new NotImplementedException("No Development for " + contentId + " " + imageId + " " + contentClass);
            }
            return Json(Resources.Resource.SuccessfullySavedCompleted, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> SearchAutoComplete(String term, String action, String controller)
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
                list = (await ProductService.SearchEntitiesAsync(whereLambda1, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
            }
            else if (isIndexAction && controller.Equals("Stories", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Story, bool>> whereLambda1 = r => r.Name.ToLower().Contains(searchKey);
                list = (await StoryService.SearchEntitiesAsync(whereLambda1, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
            }
            else if (isIndexAction && controller.Equals("ProductCategories", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<ProductCategory, bool>> whereLambda1 = r => r.Name.ToLower().Contains(searchKey);
                list = (await ProductCategoryService.SearchEntitiesAsync(whereLambda1, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
            }
            else if (isIndexAction && controller.Equals("StoryCategories", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<StoryCategory, bool>> whereLambda3 = r => r.Name.ToLower().Contains(searchKey);
                list = (await StoryCategoryService.SearchEntitiesAsync(whereLambda3, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
            }
            else if (isIndexAction &&
                  controller.Equals("Menus", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Menu, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                list = (await MenuService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
            }
            else if (isIndexAction &&
                controller.Equals("Tags", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Tag, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                list = (await TagService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
            }
            else if (isIndexAction &&
              controller.Equals("Coupons", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Coupon, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                list = (await CouponService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
            }
            else if (isIndexAction &&
               controller.Equals("TagCategories", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<TagCategory, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                list = (await TagCategoryService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
            }
            else if (isIndexAction &&
            controller.Equals("Subscribers", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Subscriber, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                list = (await SubscriberService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
            }
            else if (isIndexAction &&
         controller.Equals("Settings", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<Setting, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                list = (await SettingService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
            }
            else if (isIndexAction &&
        controller.Equals("MainPageImages", StringComparison.InvariantCultureIgnoreCase))
            {
                Expression<Func<MainPageImage, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
                list = (await MainPageImageService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
            }
            else if (isIndexAction &&
    controller.Equals("Users", StringComparison.InvariantCultureIgnoreCase))
            {
                var users = ApplicationDbContext.Users.AsQueryable();
                list = await users.Where(r => r.Email.ToLower().Contains(searchKey) || r.FirstName.ToLower().Contains(searchKey) || r.LastName.ToLower().Contains(searchKey)).Select(r => r.Email).ToListAsync();
            }
            else
            {
                throw new NotImplementedException(term + " " + action + " " + controller);
            }

            return Json(list.Take(15).ToList(), JsonRequestBehavior.AllowGet);
        }

        [DeleteAuthorize()]
        public async Task<JsonResult> SaveAdminOrderNote(int orderId, string adminOrderNote, string shipmentCompanyName = "", string shipmentTrackingNumber = "")
        {
            var order = await OrderService.GetSingleAsync(orderId);
            order.AdminOrderNote = adminOrderNote;
            order.ShipmentCompanyName = shipmentCompanyName;
            order.ShipmentTrackingNumber = shipmentTrackingNumber;
            await OrderService.SaveOrEditEntityAsync(order);
            return Json(Resources.Resource.SuccessfullySavedCompleted, JsonRequestBehavior.AllowGet);
        }

        [DeleteAuthorize()]
        public async Task<JsonResult> ChangedOrderStatus(int orderId, string orderStatus)
        {
            EImeceOrderStatus? orderStatusEnum = EnumHelper.Parse<EImeceOrderStatus>(orderStatus);
            var order = await OrderService.GetSingleAsync(orderId);
            order.OrderStatus = (int)orderStatusEnum.Value;
            await OrderService.SaveOrEditEntityAsync(order);
            return Json(Resources.Resource.SuccessfullySavedCompleted, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteTagCategoriesGridItem(List<String> values)
        {
            TagCategoryService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteCouponsGridItem(List<String> values)
        {
            CouponService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteStoryCategoryGridItem(List<String> values)
        {
            StoryCategoryService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteProductCommentGridItem(List<String> values)
        {
            ProductCommentService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteSettingGridItem(List<String> values)
        {
            SettingService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteStoryGridItem(List<String> values)
        {
            StoryService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult MainPageImageGridItem(List<String> values)
        {
            MainPageImageService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteCouponGridItem(List<String> values)
        {
            CouponService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult StoryCategoryGridItem(List<String> values)
        {
            StoryCategoryService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/Ajax
        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteSubscriberGridItem(List<String> values)
        {
            SubscriberService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteShoppingCartGridItem(List<String> values)
        {
            ShoppingCartService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteAppLogGridItem(List<String> values)
        {
            AppLogRepository.DeleteAppLogs(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteProductGridItem(List<String> values)
        {
            ProductService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteTemplateGridItem(List<String> values)
        {
            TemplateService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteTagGridItem(List<String> values)
        {
            TagService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteProductCategoriesGridItem(List<String> values)
        {
            ProductCategoryService.DeleteProductCategories(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteMainPageImageGridItem(List<String> values)
        {
            MainPageImageService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteFaqGridItem(List<String> values)
        {
            FaqService.DeleteBaseEntity(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteMenusGridItem(List<String> values)
        {
            MenuService.DeleteMenus(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public JsonResult DeleteMediaGridItem(List<String> values)
        {
            var normalizedValues = NormalizeMediaDeleteKeys(values);
            FileStorageService.DeleteBaseEntity(normalizedValues);
            return Json(normalizedValues, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Media bulk-delete keys must be fileStorageId-contentId-mod[-imageType].
        /// Older pages sent only the fileStorageId; enrich those from the media page session.
        /// </summary>
        private List<string> NormalizeMediaDeleteKeys(List<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return values ?? new List<string>();
            }

            var currentSelectedModul = Session["CurrentSelectedModul"] as Dictionary<string, string>;
            if (currentSelectedModul == null
                || !currentSelectedModul.ContainsKey("contentId")
                || !currentSelectedModul.ContainsKey("mod")
                || !currentSelectedModul.ContainsKey("imageType"))
            {
                return values;
            }

            var contentId = currentSelectedModul["contentId"];
            var mod = currentSelectedModul["mod"];
            var imageType = currentSelectedModul["imageType"];
            var normalized = new List<string>(values.Count);

            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (value.IndexOf('-') >= 0)
                {
                    normalized.Add(value);
                }
                else
                {
                    normalized.Add(string.Format("{0}-{1}-{2}-{3}", value, contentId, mod, imageType));
                }
            }

            return normalized;
        }

        [HttpPost]
        public async Task<JsonResult> ChangeMainPageImageGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await MainPageImageService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeFaqGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await FaqService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeSubscriberGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await SubscriberService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeProductCommentGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await ProductCommentService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeMediaGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await FileStorageService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeMailTemplateGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await MailTemplateService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeMenusGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await MenuService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeProductCategoriesGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await ProductCategoryService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeProductGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await ProductService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeStoryGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await StoryService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeStoryCategoryGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await StoryCategoryService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeBrandGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await BrandService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeTagGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await TagService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeCouponGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await CouponService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeTagCategoriesGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await TagCategoryService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeTemplateGridOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            await TemplateService.ChangeGridBaseEntityOrderingOrStateAsync(values, checkbox);
            return Json(new { values, checkbox }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ProductStateChanged(List<String> values, String ProductStateSelection)
        {
            int productStateValue = int.Parse(ProductStateSelection);
            ProductState state = (ProductState)productStateValue;
            ProductService.ChangeProductState(values, state);
            return Json(new { values, ProductStateSelection }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetProductTags(EImeceLanguage language, int productId = 0)
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
        }

        [HttpPost]
        public JsonResult GetStoryTags(EImeceLanguage language, int storyId = 0)
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
        }

        [HttpPost]
        public JsonResult GetProductDetailToolTip(int productId = 0)
        {
            var product = ProductService.GetProductDetailViewModelById(productId);
            var html = this.RenderPartialToString(
                        @"~/Areas/Admin/Views/Shared/pProductDetailToolTip.cshtml",
                        new ViewDataDictionary(product), null);
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetTags(EImeceLanguage language)
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
