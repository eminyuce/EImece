using Microsoft.Extensions.Logging;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models;
using EImece.Domain.Models.Enums;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class CouponValidationService : ICouponValidationService
    {
        private readonly ICouponRepository _couponRepository;
        private readonly ICouponRedemptionRepository _redemptionRepository;
        private readonly ICouponProductRepository _couponProductRepository;
        private readonly ICouponCategoryRepository _couponCategoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IOrderRepository _orderRepository;

        public CouponValidationService(ICouponRepository couponRepository,
            ICouponRedemptionRepository redemptionRepository,
            ICouponProductRepository couponProductRepository,
            ICouponCategoryRepository couponCategoryRepository,
            IProductRepository productRepository,
            IProductCategoryRepository productCategoryRepository,
            IOrderRepository orderRepository, ILogger<CouponValidationService> logger)
         {
            _couponRepository = couponRepository;
            _redemptionRepository = redemptionRepository;
            _couponProductRepository = couponProductRepository;
            _couponCategoryRepository = couponCategoryRepository;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _orderRepository = orderRepository;
        }

        [Timed("service.coupons.validate")]
        public virtual async Task<CouponValidationResult> ValidateCouponAsync(string couponCode, ShoppingCartSession cart, CouponValidationContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(couponCode))
                return CouponValidationResult.Fail(CouponValidationReason.CouponNotFound, "Coupon code required.", couponCode);
            if (cart == null)
                return CouponValidationResult.Fail(CouponValidationReason.CouponNotFound, "Cart is empty.", couponCode);

            if (context?.HasExistingCoupon == true && !string.Equals(context.ExistingCouponCode, couponCode, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await GetCouponByCodeIncludeRestrictionsAsync(context.ExistingCouponCode, context.Language, cancellationToken).ConfigureAwait(false);
                var newCouponForStack = await GetCouponByCodeIncludeRestrictionsAsync(couponCode, context.Language, cancellationToken).ConfigureAwait(false);
                bool allowStack = (existing?.AllowStacking == true) || (newCouponForStack?.AllowStacking == true);
                if (!allowStack)
                    return CouponValidationResult.Fail(CouponValidationReason.StackingNotAllowed, "Only one coupon per order is allowed.", couponCode);
            }

            var coupon = await GetCouponByCodeIncludeRestrictionsAsync(couponCode, context.Language, cancellationToken).ConfigureAwait(false);
            if (coupon == null)
                return CouponValidationResult.Fail(CouponValidationReason.CouponNotFound, "Coupon not found or expired.", couponCode);

            return await ValidateCouponInternalAsync(coupon, cart.ShoppingCartItems.Select(i => new CartItemInfo { ProductId = i.Product.Id, Quantity = i.Quantity, UnitPrice = i.Product.Price }).ToList(), cart.TotalPrice, context, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.coupons.validate_guest")]
        public virtual async Task<CouponValidationResult> ValidateCouponAsync(string couponCode, BuyWithNoAccountCreation cart, CouponValidationContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(couponCode))
                return CouponValidationResult.Fail(CouponValidationReason.CouponNotFound, "Coupon code required.", couponCode);
            if (cart == null || cart.ShoppingCartItems == null || !cart.ShoppingCartItems.Any())
                return CouponValidationResult.Fail(CouponValidationReason.CouponNotFound, "Cart is empty.", couponCode);

            var coupon = await GetCouponByCodeIncludeRestrictionsAsync(couponCode, context.Language, cancellationToken).ConfigureAwait(false);
            if (coupon == null)
                return CouponValidationResult.Fail(CouponValidationReason.CouponNotFound, "Coupon not found or expired.", couponCode);

            var items = cart.ShoppingCartItems.Select(i => new CartItemInfo { ProductId = i.Product.Id, Quantity = i.Quantity, UnitPrice = i.Product.Price }).ToList();
            return await ValidateCouponInternalAsync(coupon, items, cart.TotalPrice, context, cancellationToken).ConfigureAwait(false);
        }

        [Timed("service.coupons.revalidate")]
        public virtual async Task<CouponValidationResult> RevalidateActiveCouponAsync(ShoppingCartSession cart, CouponValidationContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cart?.Coupon == null || string.IsNullOrWhiteSpace(cart.Coupon.Code))
                return CouponValidationResult.Fail(CouponValidationReason.CouponNotFound, "No active coupon.", null);

            var freshContext = new CouponValidationContext
            {
                UserId = context.UserId,
                CustomerId = context.CustomerId,
                IsAuthenticated = context.IsAuthenticated,
                BirthDate = context.BirthDate,
                CustomerCreatedDate = context.CustomerCreatedDate,
                Language = context.Language,
                Currency = context.Currency,
                CargoPrice = context.CargoPrice,
                HasExistingCoupon = false,
                ExistingCouponCode = null
            };
            return await ValidateCouponAsync(cart.Coupon.Code, cart, freshContext, cancellationToken).ConfigureAwait(false);
        }

        private async Task<Coupon> GetCouponByCodeIncludeRestrictionsAsync(string code, int lang, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            // Controller → Service → Repository → DbContext (no direct DbContext in service)
            var coupon = await _couponRepository.FindBy(c => c.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase) && c.Lang == lang)
                .OrderBy(c => c.Position).ThenByDescending(c => c.UpdatedDate)
                .FirstOrDefaultAsync(ct).ConfigureAwait(false);
            if (coupon != null)
            {
                // Load restrictions via dedicated repositories (service orchestrates, repository does data access)
                if (_couponProductRepository != null)
                {
                    var products = await _couponProductRepository.FindBy(cp => cp.CouponId == coupon.Id).ToListAsync(ct).ConfigureAwait(false);
                    coupon.CouponProducts = products;
                }
                if (_couponCategoryRepository != null)
                {
                    var categories = await _couponCategoryRepository.FindBy(cc => cc.CouponId == coupon.Id).ToListAsync(ct).ConfigureAwait(false);
                    coupon.CouponCategories = categories;
                }
            }
            return coupon;
        }

        private class CartItemInfo
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }

        private async Task<CouponValidationResult> ValidateCouponInternalAsync(Coupon coupon, List<CartItemInfo> cartItems, decimal cartTotal, CouponValidationContext context, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            if (!coupon.IsActive)
                return CouponValidationResult.Fail(CouponValidationReason.CouponInactive, "Coupon is inactive.", coupon.Code);
            if (now < coupon.StartDate)
                return CouponValidationResult.Fail(CouponValidationReason.CouponNotYetValid, "Coupon is not yet valid.", coupon.Code);
            if (now > coupon.EndDate)
                return CouponValidationResult.Fail(CouponValidationReason.CouponExpired, "Coupon has expired.", coupon.Code);

            if (!string.IsNullOrWhiteSpace(coupon.Currency) && !string.IsNullOrWhiteSpace(context.Currency))
            {
                if (!string.Equals(coupon.Currency.Trim(), context.Currency.Trim(), StringComparison.OrdinalIgnoreCase))
                    return CouponValidationResult.Fail(CouponValidationReason.InvalidCurrency, "Coupon currency does not match order currency.", coupon.Code);
            }

            if (coupon.RequireLogin && !context.IsAuthenticated)
                return CouponValidationResult.Fail(CouponValidationReason.LoginRequired, "This coupon requires login.", coupon.Code);

            if (!string.IsNullOrWhiteSpace(coupon.AssignedUserId))
            {
                if (string.IsNullOrWhiteSpace(context.UserId))
                    return CouponValidationResult.Fail(CouponValidationReason.LoginRequired, "This coupon is assigned to a specific customer and requires login.", coupon.Code);
                if (!string.Equals(coupon.AssignedUserId, context.UserId, StringComparison.OrdinalIgnoreCase))
                    return CouponValidationResult.Fail(CouponValidationReason.AssignedToOtherCustomer, "This coupon is assigned to another customer.", coupon.Code);
            }
            if (coupon.AssignedCustomerId.HasValue)
            {
                if (!context.CustomerId.HasValue)
                    return CouponValidationResult.Fail(CouponValidationReason.LoginRequired, "This coupon is assigned to a specific customer.", coupon.Code);
                if (coupon.AssignedCustomerId.Value != context.CustomerId.Value)
                    return CouponValidationResult.Fail(CouponValidationReason.AssignedToOtherCustomer, "This coupon is assigned to another customer.", coupon.Code);
            }

            bool requiresLoginForLimits = (coupon.PerCustomerUsageLimit.HasValue && coupon.PerCustomerUsageLimit.Value > 0) || coupon.IsFirstOrderOnly || coupon.IsBirthdayCoupon || coupon.IsNewCustomerOnly;
            if (requiresLoginForLimits && !context.IsAuthenticated)
                return CouponValidationResult.Fail(CouponValidationReason.LoginRequired, "This coupon requires login.", coupon.Code);

            if (coupon.IsBirthdayCoupon)
            {
                if (!context.IsAuthenticated)
                    return CouponValidationResult.Fail(CouponValidationReason.LoginRequired, "Birthday coupon requires login.", coupon.Code);
                if (!context.BirthDate.HasValue)
                    return CouponValidationResult.Fail(CouponValidationReason.BirthdayNotEligible, "Birthday not eligible: no birth date on file.", coupon.Code);
                var birthday = context.BirthDate.Value;
                var window = coupon.BirthdayWindow ?? CouponBirthdayWindow.Month;
                bool eligible = IsBirthdayEligible(birthday, DateTime.Today, window);
                if (!eligible)
                    return CouponValidationResult.Fail(CouponValidationReason.BirthdayNotEligible, "Birthday coupon not eligible for current date.", coupon.Code);
            }

            if (coupon.IsNewCustomerOnly)
            {
                if (!string.IsNullOrEmpty(context.UserId))
                {
                    bool hasSuccessfulOrder = await HasSuccessfulOrderAsync(context.UserId, cancellationToken).ConfigureAwait(false);
                    if (hasSuccessfulOrder)
                        return CouponValidationResult.Fail(CouponValidationReason.FirstOrderOnly, "Coupon is for new customers only.", coupon.Code);
                }
                if (context.CustomerCreatedDate.HasValue)
                {
                    if ((DateTime.Now - context.CustomerCreatedDate.Value).TotalDays > 30)
                        return CouponValidationResult.Fail(CouponValidationReason.FirstOrderOnly, "Coupon is for new customers only.", coupon.Code);
                }
            }

            if (coupon.IsFirstOrderOnly)
            {
                if (string.IsNullOrEmpty(context.UserId))
                    return CouponValidationResult.Fail(CouponValidationReason.LoginRequired, "First order coupon requires login.", coupon.Code);
                bool hasSuccessfulOrder = await HasSuccessfulOrderAsync(context.UserId, cancellationToken).ConfigureAwait(false);
                if (hasSuccessfulOrder)
                    return CouponValidationResult.Fail(CouponValidationReason.FirstOrderOnly, "Coupon is valid only for first order.", coupon.Code);
            }

            var eligibleItems = await FilterEligibleItemsAsync(coupon, cartItems, cancellationToken).ConfigureAwait(false);
            if (!eligibleItems.Any())
                return CouponValidationResult.Fail(CouponValidationReason.NotApplicableToCartItems, "Coupon is not applicable to any items in your cart.", coupon.Code);

            decimal eligibleAmount = eligibleItems.Sum(i => i.UnitPrice * i.Quantity);
            var discountType = ResolveDiscountType(coupon);
            if (discountType == CouponDiscountType.Percentage && (coupon.DiscountPercentage <= 0 || coupon.DiscountPercentage > 100))
                return CouponValidationResult.Fail(CouponValidationReason.InvalidDiscount, "Invalid percentage discount.", coupon.Code);
            if (discountType == CouponDiscountType.FixedAmount && coupon.Discount <= 0)
                return CouponValidationResult.Fail(CouponValidationReason.InvalidDiscount, "Invalid fixed discount amount.", coupon.Code);
            if (discountType == CouponDiscountType.FreeShipping && coupon.IsFreeShipping == false && discountType != CouponDiscountType.FreeShipping)
                return CouponValidationResult.Fail(CouponValidationReason.InvalidDiscount, "Invalid free shipping coupon.", coupon.Code);

            if (coupon.MinimumOrderAmount.HasValue && coupon.MinimumOrderAmount.Value > 0)
            {
                if (eligibleAmount < coupon.MinimumOrderAmount.Value)
                    return CouponValidationResult.Fail(CouponValidationReason.MinOrderAmountNotMet, $"Minimum order amount of {coupon.MinimumOrderAmount.Value.CurrencySign()} not met.", coupon.Code);
            }

            if (coupon.GlobalUsageLimit.HasValue && coupon.GlobalUsageLimit.Value > 0)
            {
                int globalCount = await _redemptionRepository.GetGlobalRedemptionCountAsync(coupon.Id, cancellationToken).ConfigureAwait(false);
                if (globalCount >= coupon.GlobalUsageLimit.Value)
                    return CouponValidationResult.Fail(CouponValidationReason.UsageLimitReached, "Coupon usage limit has been reached.", coupon.Code);
            }

            if (coupon.PerCustomerUsageLimit.HasValue && coupon.PerCustomerUsageLimit.Value > 0)
            {
                if (string.IsNullOrEmpty(context.UserId) && !context.CustomerId.HasValue)
                    return CouponValidationResult.Fail(CouponValidationReason.LoginRequired, "Coupon requires login to check usage.", coupon.Code);
                int custCount = await _redemptionRepository.GetCustomerRedemptionCountAsync(coupon.Id, context.UserId, context.CustomerId, cancellationToken).ConfigureAwait(false);
                if (custCount >= coupon.PerCustomerUsageLimit.Value)
                {
                    var reason = coupon.PerCustomerUsageLimit.Value == 1 ? CouponValidationReason.AlreadyUsedByCustomer : CouponValidationReason.CustomerUsageLimitReached;
                    return CouponValidationResult.Fail(reason, "You have already used this coupon the maximum number of times.", coupon.Code);
                }
            }

            decimal discountAmount = 0;
            decimal shippingDiscount = 0;
            decimal cargoPrice = context.CargoPrice;

            if (discountType == CouponDiscountType.Percentage)
            {
                discountAmount = eligibleAmount * (coupon.DiscountPercentage / 100m);
                if (coupon.MaximumDiscountAmount.HasValue && coupon.MaximumDiscountAmount.Value > 0 && discountAmount > coupon.MaximumDiscountAmount.Value)
                    discountAmount = coupon.MaximumDiscountAmount.Value;
                discountAmount = Math.Min(discountAmount, eligibleAmount);
                discountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero);
            }
            else if (discountType == CouponDiscountType.FixedAmount)
            {
                discountAmount = Math.Min(coupon.Discount, eligibleAmount);
                discountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero);
            }
            else if (discountType == CouponDiscountType.FreeShipping)
            {
                shippingDiscount = cargoPrice;
                discountAmount = 0;
            }

            if (discountAmount > eligibleAmount) discountAmount = eligibleAmount;
            if (discountAmount < 0) discountAmount = 0;

            return CouponValidationResult.Success(coupon.Code, coupon.Id, discountAmount, shippingDiscount, eligibleAmount);
        }

        private CouponDiscountType ResolveDiscountType(Coupon coupon)
        {
            if (coupon.IsFreeShipping) return CouponDiscountType.FreeShipping;
            if (coupon.DiscountType == CouponDiscountType.FixedAmount && coupon.Discount == 0 && coupon.DiscountPercentage > 0)
                return CouponDiscountType.Percentage;
            if (coupon.DiscountType == CouponDiscountType.Percentage && coupon.DiscountPercentage == 0 && coupon.Discount > 0)
                return CouponDiscountType.FixedAmount;
            return coupon.DiscountType;
        }

        private async Task<List<CartItemInfo>> FilterEligibleItemsAsync(Coupon coupon, List<CartItemInfo> cartItems, CancellationToken ct)
        {
            if (cartItems == null || !cartItems.Any()) return new List<CartItemInfo>();

            bool hasProductRestriction = coupon.CouponProducts != null && coupon.CouponProducts.Any();
            bool hasCategoryRestriction = coupon.CouponCategories != null && coupon.CouponCategories.Any();

            HashSet<int> allowedProductIds = null;
            HashSet<int> allowedCategoryIds = null;
            if (hasProductRestriction)
                allowedProductIds = new HashSet<int>(coupon.CouponProducts.Select(cp => cp.ProductId));
            if (hasCategoryRestriction)
                allowedCategoryIds = new HashSet<int>(coupon.CouponCategories.Select(cc => cc.ProductCategoryId));

            Dictionary<int, int> productToCategory = new Dictionary<int, int>();
            if (hasCategoryRestriction || coupon.ExcludeSaleItems)
            {
                var productIds = cartItems.Select(i => i.ProductId).Distinct().ToList();
                var products = await _productRepository.FindBy(p => productIds.Contains(p.Id)).Select(p => new { p.Id, p.ProductCategoryId }).ToListAsync(ct).ConfigureAwait(false);
                productToCategory = products.ToDictionary(p => p.Id, p => p.ProductCategoryId);
            }

            HashSet<int> saleProductIds = new HashSet<int>();
            if (coupon.ExcludeSaleItems)
            {
                var productIds = cartItems.Select(i => i.ProductId).Distinct().ToList();
                var fullProducts = await _productRepository.FindBy(p => productIds.Contains(p.Id)).ToListAsync(ct).ConfigureAwait(false);
                var catIds = fullProducts.Select(p => p.ProductCategoryId).Distinct().ToList();
                var catDiscounts = await _productCategoryRepository.FindBy(c => catIds.Contains(c.Id)).Select(c => new { c.Id, c.DiscountPercantage }).ToListAsync(ct).ConfigureAwait(false);
                var catMap = catDiscounts.ToDictionary(c => c.Id, c => c.DiscountPercantage ?? 0);
                foreach (var p in fullProducts)
                {
                    bool isSale = false;
                    if (p.Discount.HasValue && p.Discount.Value > 0) isSale = true;
                    else if (catMap.ContainsKey(p.ProductCategoryId) && catMap[p.ProductCategoryId] > 0) isSale = true;
                    if (isSale) saleProductIds.Add(p.Id);
                }
            }

            var eligible = new List<CartItemInfo>();
            foreach (var item in cartItems)
            {
                if (coupon.ExcludeSaleItems && saleProductIds.Contains(item.ProductId))
                    continue;

                bool eligibleByRestriction = false;
                if (!hasProductRestriction && !hasCategoryRestriction)
                {
                    eligibleByRestriction = true;
                }
                else
                {
                    if (hasProductRestriction && allowedProductIds.Contains(item.ProductId))
                        eligibleByRestriction = true;
                    if (hasCategoryRestriction)
                    {
                        if (productToCategory.TryGetValue(item.ProductId, out var catId) && allowedCategoryIds.Contains(catId))
                            eligibleByRestriction = true;
                    }
                }

                if (eligibleByRestriction)
                    eligible.Add(item);
            }
            return eligible;
        }

        private bool IsSaleProduct(decimal price, decimal? discount, double catDiscount)
        {
            if (discount.HasValue && discount.Value > 0) return true;
            if (catDiscount > 0) return true;
            return false;
        }

        private bool IsBirthdayEligible(DateTime birthDate, DateTime today, CouponBirthdayWindow window)
        {
            int bMonth = birthDate.Month;
            int bDay = birthDate.Day;
            if (window == CouponBirthdayWindow.Month)
            {
                return bMonth == today.Month;
            }
            else
            {
                int year = today.Year;
                DateTime birthdayThisYear;
                try
                {
                    birthdayThisYear = new DateTime(year, bMonth, bDay);
                }
                catch (ArgumentOutOfRangeException)
                {
                    birthdayThisYear = new DateTime(year, 2, 28);
                }
                var diff = (birthdayThisYear - today).TotalDays;
                if (Math.Abs(diff) <= 3) return true;
                DateTime nextYear;
                DateTime prevYear;
                try { nextYear = new DateTime(year + 1, bMonth, bDay); } catch { nextYear = new DateTime(year + 1, 2, 28); }
                try { prevYear = new DateTime(year - 1, bMonth, bDay); } catch { prevYear = new DateTime(year - 1, 2, 28); }
                if (Math.Abs((nextYear - today).TotalDays) <= 3) return true;
                if (Math.Abs((prevYear - today).TotalDays) <= 3) return true;
                return false;
            }
        }

        private async Task<bool> HasSuccessfulOrderAsync(string userId, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(userId)) return false;
            int[] nonSuccessful = new[] { (int)EImeceOrderStatus.Cancelled, (int)EImeceOrderStatus.Returned, (int)EImeceOrderStatus.Refunded };
            // Repository is the only place handling DbContext; service orchestrates via repository
            return await _orderRepository.FindBy(o => o.UserId == userId && !nonSuccessful.Contains(o.OrderStatus)).AnyAsync(ct).ConfigureAwait(false);
        }
    }
}
