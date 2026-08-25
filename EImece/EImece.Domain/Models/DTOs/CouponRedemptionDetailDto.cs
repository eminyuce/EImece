using System;

namespace EImece.Domain.Models.DTOs
{
    public class CouponRedemptionDetailDto
    {
        public int Id { get; set; }
        public string CouponCode { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string UserId { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
