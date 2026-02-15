using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs
{
    public class OrderDto
    {
        // from BaseEntity
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public int Lang { get; set; }

        // from Order
        public DateTime DeliveryDate { get; set; }
        public string UserId { get; set; }
        public int OrderType { get; set; }
        public int OrderStatus { get; set; }
        public string AdminOrderNote { get; set; }
        public string OrderComments { get; set; }
        public string OrderNumber { get; set; }
        public decimal CargoPrice { get; set; }
        public int ShippingAddressId { get; set; }
        public int BillingAddressId { get; set; }
        public string OrderGuid { get; set; }
        public string Coupon { get; set; }
        public string CouponDiscount { get; set; }
        public string Token { get; set; }
        public string Price { get; set; }
        public string PaidPrice { get; set; }
        public string Installment { get; set; }
        public string Currency { get; set; }
        public string PaymentId { get; set; }
        public string PaymentStatus { get; set; }
        public int? FraudStatus { get; set; }
        public string MerchantCommissionRate { get; set; }
        public string MerchantCommissionRateAmount { get; set; }
        public string IyziCommissionRateAmount { get; set; }
        public string IyziCommissionFee { get; set; }
        public string CardType { get; set; }
        public string CardAssociation { get; set; }
        public string CardFamily { get; set; }
        public string CardToken { get; set; }
        public string CardUserKey { get; set; }
        public string BinNumber { get; set; }
        public string LastFourDigits { get; set; }
        public string BasketId { get; set; }
        public string ConversationId { get; set; }
        public string ConnectorName { get; set; }
        public string AuthCode { get; set; }
        public string HostReference { get; set; }
        public string Phase { get; set; }
        public string Status { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string Locale { get; set; }
        public long SystemTime { get; set; }
        public string ShipmentTrackingNumber { get; set; }
        public string ShipmentCompanyName { get; set; }
        public decimal PaidPriceDecimal { get; set; }
        public string InstallmentDescription { get; set; }

        // Navigation-like DTO fields for end-user views
        public CustomerDto Customer { get; set; }
        public AddressDto ShippingAddress { get; set; }
        public AddressDto BillingAddress { get; set; }
        public List<OrderProductDto> OrderProducts { get; set; } = new List<OrderProductDto>();
    }
}
