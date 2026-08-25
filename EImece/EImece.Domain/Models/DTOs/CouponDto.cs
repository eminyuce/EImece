using System;
using EImece.Domain.Models.Enums;

namespace EImece.Domain.Models.DTOs
{
    public class CouponDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from Coupon
        public string Code { get; set; }
        public int DiscountPercentage { get; set; }
        public int Discount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartDateStr { get; set; }
        public string EndDateStr { get; set; }
        public string AssignedUserId { get; set; }
        public int? AssignedCustomerId { get; set; }

        public CouponDiscountType DiscountType { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public int? GlobalUsageLimit { get; set; }
        public int? PerCustomerUsageLimit { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public bool ExcludeSaleItems { get; set; }
        public bool IsFreeShipping { get; set; }
        public bool AllowStacking { get; set; }
        public bool RequireLogin { get; set; }
        public bool IsFirstOrderOnly { get; set; }
        public bool IsNewCustomerOnly { get; set; }
        public bool IsBirthdayCoupon { get; set; }
        public CouponBirthdayWindow? BirthdayWindow { get; set; }
        public string Currency { get; set; }
    }
}
