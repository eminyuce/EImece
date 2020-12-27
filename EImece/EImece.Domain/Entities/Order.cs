using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EImece.Domain.Entities
{
    public class Order : BaseEntity
    {
        public DateTime DeliveryDate { get; set; }
        public string UserId { get; set; }
        public int OrderStatus { get; set; }
        public string OrderComments { get; set; }
        public string OrderNumber { get; set; }
        public double CargoPrice { get; set; }
        public int ShippingAddressId { get; set; }
        public int BillingAddressId { get; set; }
        public string OrderGuid { get; set; }
        public string Coupon { get; set; }
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

        public Address ShippingAddress { get; set; }
        public Address BillingAddress { get; set; }
        public ICollection<OrderProduct> OrderProducts { get; set; }

        [NotMapped]
        public Customer Customer { get; set; }

        [NotMapped]
        public decimal PaidPriceDecimal
        {
            get
            {
                return decimal.Round(PaidPrice.ToDecimal(), 3, MidpointRounding.AwayFromZero);
            }
        }
    }
}