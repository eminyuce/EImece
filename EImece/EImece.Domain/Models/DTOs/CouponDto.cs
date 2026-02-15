using System;

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
    }
}
