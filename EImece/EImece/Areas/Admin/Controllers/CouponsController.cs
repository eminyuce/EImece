using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Filters;
using Griddly.Mvc;
using Griddly.Mvc.Results;
using NLog;
using Resources;
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Services.IServices;

namespace EImece.Areas.Admin.Controllers
{
    public class CouponsController : BaseAdminController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected ICouponService CouponService { get; }
        protected IEntityFactory EntityFactory { get; }
        protected ICustomerService CustomerService { get; }
        protected IProductService ProductService { get; }
        protected IProductCategoryService ProductCategoryService { get; }

        public CouponsController(
            ISettingService settingService,
            ICouponService couponService,
            IEntityFactory entityFactory,
            ICustomerService customerService,
            IProductService productService,
            IProductCategoryService productCategoryService)
            : base(settingService)
        {
            CouponService = couponService ?? throw new ArgumentNullException(nameof(couponService));
            EntityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
            CustomerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            ProductService = productService ?? throw new ArgumentNullException(nameof(productService));
            ProductCategoryService = productCategoryService ?? throw new ArgumentNullException(nameof(productCategoryService));
        }

        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken, String search = "")
        {
            Expression<Func<Coupon, bool>> whereLambda = r => r.Name.Contains(search) || r.Code.Contains(search);
            var result = await CouponService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return View(result);
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> IndexGrid(CancellationToken cancellationToken, String search = "")
        {
            if (!CanRenderGrid())
            {
                return RedirectToAction("Index", new { search });
            }

            Expression<Func<Coupon, bool>> whereLambda = r => r.Name.Contains(search) || r.Code.Contains(search);
            var result = await CouponService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage);
            return new QueryableResult<Coupon>(result.AsQueryable());
        }

        public async Task<ActionResult> SaveOrEdit(CancellationToken cancellationToken, int id = 0)
        {
            var content = EntityFactory.GetBaseEntityInstance<Coupon>();
            if (id == 0)
            {
                content.StartDate = DateTime.Now;
                content.EndDate = DateTime.Now.AddMonths(1);
                content.StartDateStr = content.StartDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                content.EndDateStr = content.EndDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                content = await CouponService.GetSingleAsync(id);
                if (content != null)
                {
                    content.StartDateStr = content.StartDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    content.EndDateStr = content.EndDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    try
                    {
                        var prodIds = await CouponService.GetCouponProductIdsAsync(id, cancellationToken).ConfigureAwait(false);
                        content.ProductIdsCsv = string.Join(",", prodIds);
                        var catIds = await CouponService.GetCouponCategoryIdsAsync(id, cancellationToken).ConfigureAwait(false);
                        content.CategoryIdsCsv = string.Join(",", catIds);
                        var total = await CouponService.GetRedemptionCountAsync(id, cancellationToken).ConfigureAwait(false);
                        ViewBag.TotalRedemptions = total;
                        if (content.GlobalUsageLimit.HasValue && content.GlobalUsageLimit.Value > 0)
                            ViewBag.RemainingRedemptions = content.GlobalUsageLimit.Value - total;
                        var recent = await CouponService.GetRecentRedemptionsAsync(id, 20, cancellationToken).ConfigureAwait(false);
                        ViewBag.Redemptions = recent;
                    }
                    catch (Exception ex) { Logger.Warn(ex, "Failed to load coupon details"); }
                }
            }
            try
            {
                // Use ProductService and ProductCategoryService via service layer (no DbContext in controller)
                var products = await ProductService.SearchEntitiesAsync(r => r.Name.Contains("") , "", CurrentLanguage).ConfigureAwait(false);
                ViewBag.AllProducts = products.OrderBy(p => p.Name).Select(p => new { p.Id, p.Name }).Take(500).ToList();
                var categories = await ProductCategoryService.SearchEntitiesAsync(r => r.Name.Contains(""), "", CurrentLanguage).ConfigureAwait(false);
                ViewBag.AllCategories = categories.OrderBy(c => c.Name).Select(c => new { c.Id, c.Name }).Take(500).ToList();
            }
            catch (Exception ex) { Logger.Warn(ex, "Failed to load product/category lookups"); ViewBag.AllProducts = new object[0]; ViewBag.AllCategories = new object[0]; }
            try
            {
                ViewBag.AllCustomers = await CustomerService.GetAllAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to load customer lookup");
                ViewBag.AllCustomers = new EImece.Domain.Entities.Customer[0];
            }
            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveOrEdit(Coupon coupon, String saveButton = null)
        {
            try
            {
                if (coupon == null) return HttpNotFound();
                if (!TryParseAndValidateDates(coupon)) return View(coupon);

                MapDiscountType(coupon);
                coupon.Lang = CurrentLanguage == 0 ? CurrentLanguage : CurrentLanguage;

                await CouponService.SaveOrEditEntityAsync(coupon).ConfigureAwait(false);
                await TrySaveRestrictionsAsync(coupon);

                if (ShouldRedirectToIndex(saveButton)) return RedirectToAction("Index");

                ModelState.AddModelError("", AdminResource.SuccessfullySavedCompleted);
                RefreshDateStrings(coupon);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to save changes:" + ex.StackTrace, coupon);
                ModelState.AddModelError("", AdminResource.GeneralSaveErrorMessage + "  " + ex.StackTrace + ex.Message);
            }
            await PopulateCouponEditViewBagsAsync(coupon);
            RemoveModelState();
            return View(coupon);
        }

        private bool TryParseAndValidateDates(Coupon coupon)
        {
            coupon.StartDate = coupon.StartDateStr.ToDateTime();
            coupon.EndDate = coupon.EndDateStr.ToDateTime();
            if (coupon.EndDate <= coupon.StartDate)
            {
                ModelState.AddModelError("EndDateStr", AdminResource.EndDateBiggerThanStartDateText);
                return false;
            }
            return true;
        }

        private static void MapDiscountType(Coupon coupon)
        {
            if (coupon.IsFreeShipping) coupon.DiscountType = EImece.Domain.Models.Enums.CouponDiscountType.FreeShipping;
            else if (coupon.Discount > 0 && coupon.DiscountPercentage == 0) coupon.DiscountType = EImece.Domain.Models.Enums.CouponDiscountType.FixedAmount;
            else if (coupon.DiscountPercentage > 0) coupon.DiscountType = EImece.Domain.Models.Enums.CouponDiscountType.Percentage;
        }

        private async Task TrySaveRestrictionsAsync(Coupon coupon)
        {
            try { await CouponService.SaveCouponRestrictionsAsync(coupon.Id, coupon.ProductIdsCsv, coupon.CategoryIdsCsv).ConfigureAwait(false); }
            catch (Exception ex) { Logger.Warn(ex, "Failed to save coupon restrictions"); }
        }

        private static bool ShouldRedirectToIndex(string saveButton)
        {
            return !String.IsNullOrEmpty(saveButton) && saveButton.Equals(AdminResource.SaveButtonAndCloseText, StringComparison.InvariantCultureIgnoreCase);
        }

        private static void RefreshDateStrings(Coupon coupon)
        {
            coupon.StartDateStr = coupon.StartDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            coupon.EndDateStr = coupon.EndDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        private async Task PopulateCouponEditViewBagsAsync(Coupon coupon)
        {
            try
            {
                var products = await ProductService.SearchEntitiesAsync(r => r.Name.Contains(""), "", CurrentLanguage).ConfigureAwait(false);
                ViewBag.AllProducts = products.OrderBy(p => p.Name).Select(p => new { p.Id, p.Name }).Take(500).ToList();
                var categories = await ProductCategoryService.SearchEntitiesAsync(r => r.Name.Contains(""), "", CurrentLanguage).ConfigureAwait(false);
                ViewBag.AllCategories = categories.OrderBy(c => c.Name).Select(c => new { c.Id, c.Name }).Take(500).ToList();
                if (coupon != null && coupon.Id > 0)
                {
                    var total = await CouponService.GetRedemptionCountAsync(coupon.Id).ConfigureAwait(false);
                    ViewBag.TotalRedemptions = total;
                    if (coupon.GlobalUsageLimit.HasValue && coupon.GlobalUsageLimit.Value > 0)
                        ViewBag.RemainingRedemptions = coupon.GlobalUsageLimit.Value - total;
                }
            }
            catch { }
            try { ViewBag.AllCustomers = await CustomerService.GetAllAsync().ConfigureAwait(false); }
            catch (Exception ex) { Logger.Warn(ex, "Failed to load customer lookup"); ViewBag.AllCustomers = new EImece.Domain.Entities.Customer[0]; }
        }

        [HttpGet]
        public async Task<ActionResult> GenerateForCustomer(int? customerId, string customerUserId)
        {
            ViewBag.Customers = await CustomerService.GetAllAsync().ConfigureAwait(false);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GenerateForCustomer(string baseCode, int discount, int discountPercentage, string assignedUserId, int? assignedCustomerId, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(baseCode))
                baseCode = "CPN" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            var coupon = EntityFactory.GetBaseEntityInstance<Coupon>();
            coupon.Name = $"Customer coupon {baseCode}";
            coupon.Code = baseCode.ToUpper().Trim();
            coupon.Discount = discount;
            coupon.DiscountPercentage = discountPercentage;
            coupon.StartDate = startDate ?? DateTime.Now;
            coupon.EndDate = endDate ?? DateTime.Now.AddMonths(1);
            coupon.StartDateStr = coupon.StartDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            coupon.EndDateStr = coupon.EndDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            coupon.IsActive = true;
            coupon.Lang = CurrentLanguage;
            coupon.AssignedUserId = string.IsNullOrWhiteSpace(assignedUserId) ? null : assignedUserId.Trim();
            coupon.AssignedCustomerId = assignedCustomerId;
            var existingList = await CouponService.SearchEntitiesAsync(r => r.Code == coupon.Code, coupon.Code, CurrentLanguage).ConfigureAwait(false);
            var existing = existingList.FirstOrDefault();
            if (existing != null)
            {
                ModelState.AddModelError("Code", "Code already exists, try another");
                ViewBag.Customers = await CustomerService.GetAllAsync().ConfigureAwait(false);
                return View(coupon);
            }
            await CouponService.SaveOrEditEntityAsync(coupon).ConfigureAwait(false);
            SetSuccessMessage($"Coupon {coupon.Code} generated for customer {(assignedUserId ?? assignedCustomerId?.ToString() ?? "global")}");
            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [DeleteAuthorize()]
        public async Task<ActionResult> DeleteConfirmed(CancellationToken cancellationToken, int id)
        {
            var item = await CouponService.GetSingleAsync(id).ConfigureAwait(false);
            if (item == null)
            {
                return HttpNotFound();
            }
            try
            {
                await CouponService.DeleteEntityAsync(item).ConfigureAwait(false);
                SetSuccessMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Unable to delete product:" + ex.StackTrace, item);
                SetErrorMessage();
                return ReturnIndexIfNotUrlReferrer("Index");
            }
        }

        [HttpGet]
        public async Task<ActionResult> Redemptions(int? id, CancellationToken cancellationToken)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var couponId = id.Value;
            var coupon = await CouponService.GetSingleAsync(couponId).ConfigureAwait(false);
            if (coupon == null) return HttpNotFound();
            var redemptions = await CouponService.GetRedemptionsWithDetailsAsync(couponId, 100, cancellationToken).ConfigureAwait(false);
            ViewBag.Coupon = coupon;
            return View(redemptions);
        }

        [HttpGet, ActionName("ExportExcel")]
        public async Task<ActionResult> ExportExcelAsync(CancellationToken cancellationToken, string format = "excel")
        {
            return await DownloadFileAsync(format).ConfigureAwait(false);
        }

        private async Task<ActionResult> DownloadFileAsync(string format = "excel")
        {
            String search = "";
            Expression<Func<Coupon, bool>> whereLambda = r => r.Name.Contains(search) || r.Code.Contains(search);
            var Coupons = await CouponService.SearchEntitiesAsync(whereLambda, search, CurrentLanguage).ConfigureAwait(false);

            var result = from r in Coupons
                         select new
                         {
                             Id = r.Id,
                             Name = r.Name.ToStr(250),
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate,
                             IsActive = r.IsActive,
                             Position = r.Position,
                         };

            return DownloadFile(result, String.Format("Coupons-{0}", GetCurrentLanguage), format);
        }
    }
}
