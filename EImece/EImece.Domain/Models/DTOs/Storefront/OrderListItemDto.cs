using EImece.Domain.Helpers;
using System;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Minimal order row for customer order list page. Only fields rendered in CustomerOrders.cshtml.
    /// Query: SELECT Id, OrderNumber, OrderStatus, CreatedDate, PaidPrice, ShipmentTrackingNumber, ShipmentCompanyName FROM Orders WHERE UserId=@id
    /// </summary>
    public class OrderListItemDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int OrderStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public string PaidPrice { get; set; }
        public string ShipmentTrackingNumber { get; set; }
        public string ShipmentCompanyName { get; set; }

        public decimal PaidPriceDecimal
        {
            get { return decimal.Round(PaidPrice.ToDecimal(), 3, MidpointRounding.AwayFromZero); }
        }
    }
}
