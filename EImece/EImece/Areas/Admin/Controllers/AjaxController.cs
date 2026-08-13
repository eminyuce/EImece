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
using EImece.Domain.DependencyInjection;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using NLog;

namespace EImece.Areas.Admin.Controllers
{
    [ValidateJsonAntiForgeryToken]
    public class AjaxController : BaseAdminController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        [Inject]
        public ApplicationDbContext ApplicationDbContext { get; set; }

        private AppLogRepository AppLogRepository { get; set; }

        [Inject]
        public IShoppingCartService ShoppingCartService { get; set; }

        [Inject]
        public UsersService UsersService { get; set; }

        [Inject]
        public ICustomerService CustomerService { get; set; }

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
                catch (Exception fallbackEx)
                {
                    // Ignore: NLog and Trace both failed; further logging from OnException could recurse.
                    Logger.Debug(fallbackEx, "AjaxController OnException fallback Trace.TraceError failed.");
                }
            }

            base.OnException(filterContext);
        }

        [HttpPost]
        public async Task<ActionResult> UpdatePrices(UpdatePriceRequest request)
        {
            if (!IsProductPriceEnabled)
            {
                Logger.Warn("UpdatePrices blocked because IsProductPriceEnable is false");
                return Json(new { success = false, message = "Fiyat işlemleri devre dışı." }, JsonRequestBehavior.AllowGet);
            }

            Logger.Info($"UpdatePrices called by {User?.Identity?.Name ?? "-"} with Percentage={request?.PercentageOfIncreaseOrDecrease}, ProductId={request?.ProductId}, CategoryId={request?.CategoryId}, BrandId={request?.BrandId}, TagId={request?.TagId}");
            try
            {
                if (request == null || request.PercentageOfIncreaseOrDecrease == null)
                {
                    Logger.Warn("UpdatePrices: missing percentage in request");
                    return Json(new { success = false, message = "Yüzde değeri gerekli." }, JsonRequestBehavior.AllowGet);
                }
                var affectedRows = await ProductService.UpdatePricesAsync(request);
                Logger.Info($"UpdatePrices completed. AffectedRows={affectedRows}");
                // Başarılı yanıt döndür
                return Json(new { success = true, affectedRows = affectedRows }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "UpdatePrices failed");
                return Json(new { success = false, message = "İşlem başarısız." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public async Task<JsonResult> DeleteBaseContentMainImage(int contentId, int imageId, String contentClass)
        {
            Logger.Info($"DeleteBaseContentMainImage called by {User?.Identity?.Name ?? "-"} ContentId={contentId} ImageId={imageId} ContentClass={contentClass}");
            if (string.IsNullOrEmpty(contentClass))
            {
                Logger.Warn("DeleteBaseContentMainImage: contentClass is empty");
                return Json("Error contentClassName does not exists", JsonRequestBehavior.AllowGet);
            }

            if (contentClass.Equals(typeof(Product).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                await FileStorageService.DeleteFileStorageAsync(imageId);
                var item = await ProductService.GetSingleAsync(contentId);
                item.MainImageId = null;
                await ProductService.SaveOrEditEntityAsync(item);
            }
            else if (contentClass.Equals(typeof(Menu).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                await FileStorageService.DeleteFileStorageAsync(imageId);
                var item = await MenuService.GetSingleAsync(contentId);
                item.MainImageId = null;
                await MenuService.SaveOrEditEntityAsync(item);
            }
            else if (contentClass.Equals(typeof(ProductCategory).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                await FileStorageService.DeleteFileStorageAsync(imageId);
                var item = await ProductCategoryService.GetSingleAsync(contentId);
                item.MainImageId = null;
                await ProductCategoryService.SaveOrEditEntityAsync(item);
            }
            else if (contentClass.Equals(typeof(Story).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                await FileStorageService.DeleteFileStorageAsync(imageId);
                var item = await StoryService.GetSingleAsync(contentId);
                item.MainImageId = null;
                await StoryService.SaveOrEditEntityAsync(item);
            }
            else if (contentClass.Equals(typeof(StoryCategory).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                await FileStorageService.DeleteFileStorageAsync(imageId);
                var item = await StoryCategoryService.GetSingleAsync(contentId);
                item.MainImageId = null;
                await StoryCategoryService.SaveOrEditEntityAsync(item);
            }
            else if (contentClass.Equals(typeof(MainPageImage).Name, StringComparison.InvariantCultureIgnoreCase))
            {
                await FileStorageService.DeleteFileStorageAsync(imageId);
                var item = await MainPageImageService.GetSingleAsync(contentId);
                item.MainImageId = null;
                await MainPageImageService.SaveOrEditEntityAsync(item);
            }
            else
            {
                throw new NotImplementedException("No Development for " + contentId + " " + imageId + " " + contentClass);
            }
            return Json(Resources.Resource.SuccessfullySavedCompleted, JsonRequestBehavior.AllowGet);
        }

        // Parameter names must NOT be "action"/"controller" — those collide with MVC route tokens
        // and bind as SearchAutoComplete/Ajax instead of the page context from the client.
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<JsonResult> SearchAutoComplete(string term, string actionName, string controllerName)
        {
            if (string.IsNullOrEmpty(term) || string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
            {
                throw new ArgumentException("term or actionName or controllerName cannot be empty");
            }
            String searchKey = term.ToStr().ToLower(CultureInfo.InvariantCulture).Trim();
            Boolean isIndexAction = actionName.Equals("Index", StringComparison.InvariantCultureIgnoreCase);
            var list = await SearchIndexActionAsync(isIndexAction, controllerName, searchKey, term, actionName);
            return Json(list.Take(15).ToList(), JsonRequestBehavior.AllowGet);
        }

        private async Task<List<string>> SearchIndexActionAsync(bool isIndexAction, string controllerName, string searchKey, string term, string actionName)
        {
            if (!isIndexAction)
            {
                throw new NotImplementedException(term + " " + actionName + " " + controllerName);
            }

            var list = await SearchIndexByControllerAsync(controllerName, searchKey);
            if (list == null)
            {
                throw new NotImplementedException(term + " " + actionName + " " + controllerName);
            }

            return list;
        }

        private async Task<List<string>> SearchIndexByControllerAsync(string controllerName, string searchKey)
        {
            if (IsController(controllerName, "Products"))
            {
                return await SearchProductsIndexAsync(searchKey);
            }
            if (IsController(controllerName, "Stories"))
            {
                return await SearchStoriesIndexAsync(searchKey);
            }
            if (IsController(controllerName, "ProductCategories"))
            {
                return await SearchProductCategoriesIndexAsync(searchKey);
            }
            if (IsController(controllerName, "StoryCategories"))
            {
                return await SearchStoryCategoriesIndexAsync(searchKey);
            }
            if (IsController(controllerName, "Menus"))
            {
                return await SearchMenusIndexAsync(searchKey);
            }
            if (IsController(controllerName, "Tags"))
            {
                return await SearchTagsIndexAsync(searchKey);
            }
            if (IsController(controllerName, "Coupons"))
            {
                return await SearchCouponsIndexAsync(searchKey);
            }
            if (IsController(controllerName, "TagCategories"))
            {
                return await SearchTagCategoriesIndexAsync(searchKey);
            }
            if (IsController(controllerName, "Subscribers"))
            {
                return await SearchSubscribersIndexAsync(searchKey);
            }
            if (IsController(controllerName, "Settings"))
            {
                return await SearchSettingsIndexAsync(searchKey);
            }
            if (IsController(controllerName, "MainPageImages"))
            {
                return await SearchMainPageImagesIndexAsync(searchKey);
            }
            if (IsController(controllerName, "Users"))
            {
                return await SearchUsersIndexAsync(searchKey);
            }

            return null;
        }

        private static bool IsController(string controllerName, string name)
        {
            return controllerName.Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }

        private async Task<List<string>> SearchProductsIndexAsync(string searchKey)
        {
            Expression<Func<Product, bool>> whereLambda1 = r =>
                r.Name.ToLower().Contains(searchKey)
                || (r.ProductCode != null && r.ProductCode.ToLower().Contains(searchKey))
                || (r.NameLong != null && r.NameLong.ToLower().Contains(searchKey))
                || (r.NameShort != null && r.NameShort.ToLower().Contains(searchKey));
            var products = await ProductService.SearchEntitiesAsync(whereLambda1, searchKey, CurrentLanguage);
            return products
                .SelectMany(r => new[] { r.Name, r.ProductCode })
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<List<string>> SearchStoriesIndexAsync(string searchKey)
        {
            Expression<Func<Story, bool>> whereLambda1 = r => r.Name.ToLower().Contains(searchKey);
            return (await StoryService.SearchEntitiesAsync(whereLambda1, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
        }

        private async Task<List<string>> SearchProductCategoriesIndexAsync(string searchKey)
        {
            Expression<Func<ProductCategory, bool>> whereLambda1 = r => r.Name.ToLower().Contains(searchKey);
            return (await ProductCategoryService.SearchEntitiesAsync(whereLambda1, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
        }

        private async Task<List<string>> SearchStoryCategoriesIndexAsync(string searchKey)
        {
            Expression<Func<StoryCategory, bool>> whereLambda3 = r => r.Name.ToLower().Contains(searchKey);
            return (await StoryCategoryService.SearchEntitiesAsync(whereLambda3, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
        }

        private async Task<List<string>> SearchMenusIndexAsync(string searchKey)
        {
            Expression<Func<Menu, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
            return (await MenuService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
        }

        private async Task<List<string>> SearchTagsIndexAsync(string searchKey)
        {
            Expression<Func<Tag, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
            return (await TagService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
        }

        private async Task<List<string>> SearchCouponsIndexAsync(string searchKey)
        {
            Expression<Func<Coupon, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
            return (await CouponService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
        }

        private async Task<List<string>> SearchTagCategoriesIndexAsync(string searchKey)
        {
            Expression<Func<TagCategory, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
            return (await TagCategoryService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
        }

        private async Task<List<string>> SearchSubscribersIndexAsync(string searchKey)
        {
            Expression<Func<Subscriber, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
            return (await SubscriberService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
        }

        private async Task<List<string>> SearchSettingsIndexAsync(string searchKey)
        {
            Expression<Func<Setting, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
            return (await SettingService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
        }

        private async Task<List<string>> SearchMainPageImagesIndexAsync(string searchKey)
        {
            Expression<Func<MainPageImage, bool>> whereLamba5 = r => r.Name.ToLower().Contains(searchKey);
            return (await MainPageImageService.SearchEntitiesAsync(whereLamba5, searchKey, CurrentLanguage)).Select(r => r.Name).ToList();
        }

        private async Task<List<string>> SearchUsersIndexAsync(string searchKey)
        {
            var users = ApplicationDbContext.Users.AsQueryable();
            return await users.Where(r => r.Email.ToLower().Contains(searchKey) || r.FirstName.ToLower().Contains(searchKey) || r.LastName.ToLower().Contains(searchKey)).Select(r => r.Email).ToListAsync();
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
        public async Task<JsonResult> DeleteTagCategoriesGridItem(List<String> values)
        {
            await TagCategoryService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteCouponsGridItem(List<String> values)
        {
            await CouponService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteStoryCategoryGridItem(List<String> values)
        {
            await StoryCategoryService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteProductCommentGridItem(List<String> values)
        {
            await ProductCommentService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteSettingGridItem(List<String> values)
        {
            await SettingService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteStoryGridItem(List<String> values)
        {
            await StoryService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> MainPageImageGridItem(List<String> values)
        {
            await MainPageImageService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteCouponGridItem(List<String> values)
        {
            return await DeleteCouponsGridItem(values);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> StoryCategoryGridItem(List<String> values)
        {
            return await DeleteStoryCategoryGridItem(values);
        }

        // GET: Admin/Ajax
        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteSubscriberGridItem(List<String> values)
        {
            await SubscriberService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteShoppingCartGridItem(List<String> values)
        {
            await ShoppingCartService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteOrdersGridItem(List<String> values)
        {
            await OrderService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteAppLogGridItem(List<String> values)
        {
            await AppLogRepository.DeleteAppLogsAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteProductGridItem(List<String> values)
        {
            await ProductService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteTemplateGridItem(List<String> values)
        {
            await TemplateService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteTagGridItem(List<String> values)
        {
            await TagService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Bulk-delete users selected on Users / CustomerRoles grids (grid name: UsersGrid).
        /// Only Customer-role accounts are removed; related customer/order rows are cleaned up.
        /// </summary>
        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteUsersGridItem(List<String> values)
        {
            var deleted = new List<string>();
            if (values == null || values.Count == 0)
            {
                return Json(deleted, JsonRequestBehavior.AllowGet);
            }

            var currentUserId = User?.Identity?.GetUserId();
            foreach (var userId in values.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(currentUserId)
                    && string.Equals(currentUserId, userId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var user = await ApplicationDbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    continue;
                }

                var roles = await UserManager.GetRolesAsync(userId);
                var isCustomer = roles != null
                    && roles.Any(r => r.Equals(Domain.Constants.CustomerRole, StringComparison.OrdinalIgnoreCase));
                if (!isCustomer)
                {
                    // Safety: customer grid must not bulk-delete staff accounts.
                    continue;
                }

                await CustomerService.DeleteByUserIdAsync(userId);
                await OrderService.DeleteByUserIdAsync(userId);
                await UsersService.DeleteUserAsync(userId);
                deleted.Add(userId);
            }

            return Json(deleted, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteProductCategoriesGridItem(List<String> values)
        {
            await ProductCategoryService.DeleteProductCategoriesAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteMainPageImageGridItem(List<String> values)
        {
            return await MainPageImageGridItem(values);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteFaqGridItem(List<String> values)
        {
            await FaqService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteMenusGridItem(List<String> values)
        {
            await MenuService.DeleteMenusAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteMediaGridItem(List<String> values)
        {
            var normalizedValues = NormalizeMediaDeleteKeys(values);
            await FileStorageService.DeleteBaseEntityAsync(normalizedValues);
            return Json(normalizedValues, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteBrandGridItem(List<String> values)
        {
            if (values == null || values.Count == 0)
            {
                return Json(new List<String>(), JsonRequestBehavior.AllowGet);
            }

            foreach (var value in values)
            {
                await BrandService.DeleteBrandByIdAsync(value.ToInt());
            }

            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteListGridItem(List<String> values)
        {
            if (values == null || values.Count == 0)
            {
                return Json(new List<String>(), JsonRequestBehavior.AllowGet);
            }

            foreach (var value in values)
            {
                await ListService.DeleteListByIdAsync(value.ToInt());
            }

            return Json(values, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [DeleteAuthorize()]
        public async Task<JsonResult> DeleteMailTemplateGridItem(List<String> values)
        {
            await MailTemplateService.DeleteBaseEntityAsync(values);
            return Json(values, JsonRequestBehavior.AllowGet);
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
        public async Task<JsonResult> ProductStateChanged(List<String> values, String ProductStateSelection)
        {
            int productStateValue = int.Parse(ProductStateSelection);
            ProductState state = (ProductState)productStateValue;
            await ProductService.ChangeProductStateAsync(values, state);
            return Json(new { values, ProductStateSelection }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> GetProductTags(EImeceLanguage language, int productId = 0)
        {
            var tags = await TagCategoryService.GetTagsByTagTypeAsync(language);
            if (tags.IsEmpty())
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                tags = EntityFilterHelper.FilterTagCategories(tags);
                var productTags = (await ProductService.GetProductTagsByProductIdAsync(productId)).Select(r => r.TagId).ToList();
                var tempData = new TempDataDictionary();
                tempData["selectedTags"] = productTags;
                var html = this.RenderPartialToString(
                            @"~/Areas/Admin/Views/Shared/pSelectedTags.cshtml",
                            new ViewDataDictionary(tags), tempData);
                return Json(html, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetStoryTags(EImeceLanguage language, int storyId = 0)
        {
            var tags = await TagCategoryService.GetTagsByTagTypeAsync(language);
            if (tags.IsEmpty())
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                tags = EntityFilterHelper.FilterTagCategories(tags);
                var storyTags = (await StoryService.GetStoryTagsByStoryIdAsync(storyId)).Select(r => r.TagId).ToList();
                var tempData = new TempDataDictionary();
                tempData["selectedTags"] = storyTags;
                var html = this.RenderPartialToString(
                            @"~/Areas/Admin/Views/Shared/pSelectedTags.cshtml",
                            new ViewDataDictionary(tags), tempData);
                return Json(html, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetProductDetailToolTip(int productId = 0)
        {
            var product = await ProductService.GetProductDetailViewModelByIdAsync(productId);
            var html = this.RenderPartialToString(
                        @"~/Areas/Admin/Views/Shared/pProductDetailToolTip.cshtml",
                        new ViewDataDictionary(product), null);
            return Json(html, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> GetTags(EImeceLanguage language)
        {
            var tags = await TagCategoryService.GetTagsByTagTypeAsync(language);
            var tempData = new TempDataDictionary();
            var html = this.RenderPartialToString(
                        @"~/Areas/Admin/Views/Shared/pImagesTag.cshtml",
                        new ViewDataDictionary(tags), tempData);
            return Json(html, JsonRequestBehavior.AllowGet);
        }
    }
}
