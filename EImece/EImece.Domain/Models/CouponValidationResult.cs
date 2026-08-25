using EImece.Domain.Models.Enums;
using System;

namespace EImece.Domain.Models
{
    public class CouponValidationResult
    {
        public bool IsValid { get; set; }
        public CouponValidationReason Reason { get; set; }
        public string Message { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingDiscount { get; set; }
        public decimal EligibleAmount { get; set; }
        public string CouponCode { get; set; }
        public int? CouponId { get; set; }

        public static CouponValidationResult Success(string code, int couponId, decimal discount, decimal shippingDiscount, decimal eligibleAmount)
        {
            return new CouponValidationResult
            {
                IsValid = true,
                Reason = CouponValidationReason.Valid,
                Message = "Coupon applied successfully.",
                DiscountAmount = discount,
                ShippingDiscount = shippingDiscount,
                EligibleAmount = eligibleAmount,
                CouponCode = code,
                CouponId = couponId
            };
        }

        public static CouponValidationResult Fail(CouponValidationReason reason, string message, string code = null)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                Reason = reason,
                Message = message,
                DiscountAmount = 0,
                ShippingDiscount = 0,
                EligibleAmount = 0,
                CouponCode = code
            };
        }
    }

    public class CouponValidationContext
    {
        public string UserId { get; set; }
        public int? CustomerId { get; set; }
        public bool IsAuthenticated { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? CustomerCreatedDate { get; set; }
        public int Language { get; set; }
        public string Currency { get; set; }
        public bool HasExistingCoupon { get; set; }
        public string ExistingCouponCode { get; set; }
        public decimal CargoPrice { get; set; }
    }
}
