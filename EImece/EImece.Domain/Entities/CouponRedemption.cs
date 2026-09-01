using System;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class CouponRedemption : BaseEntity
    {
        public int CouponId { get; set; }
        public virtual Coupon Coupon { get; set; }

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public string UserId { get; set; }

        public string CouponCode { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal OrderTotalBeforeDiscount { get; set; }

        public string Currency { get; set; }

        // Shadow BaseEntity Name = CouponCode for display
    }
}
