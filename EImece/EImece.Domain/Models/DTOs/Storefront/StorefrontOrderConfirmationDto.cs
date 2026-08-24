using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using Resources;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.DTOs.Storefront
{
    /// <summary>
    /// Purpose-built DTO for ThankYouForYourOrder.cshtml and CargoTrackingResult.cshtml.
    /// Contains only the fields those two views render.
    /// Query: SELECT <18 scalar cols> FROM Orders (+3 correlated lookups: customer, shipping address, billing address,
    /// and OrderProducts line items: ProductName, Quantity, ProductSalePrice, TotalPrice).
    /// </summary>
    public class StorefrontOrderConfirmationDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public int OrderStatus { get; set; }

        // payment summary block
        public decimal CargoPrice { get; set; }
        public string Coupon { get; set; }
        public string CouponDiscount { get; set; }
        public string Price { get; set; }
        public string PaidPrice { get; set; }
        public string Installment { get; set; }
        public string CardFamily { get; set; }
        public string CardType { get; set; }
        public string CardAssociation { get; set; }
        public string LastFourDigits { get; set; }

        // cargo tracking block
        public string ShipmentCompanyName { get; set; }
        public string ShipmentTrackingNumber { get; set; }
        public string AdminOrderNote { get; set; }
        public string OrderComments { get; set; }

        public List<StorefrontOrderConfirmationItemDto> OrderProducts { get; set; } = new List<StorefrontOrderConfirmationItemDto>();

        public StorefrontOrderConfirmationAddressDto ShippingAddress { get; set; }
        public StorefrontOrderConfirmationAddressDto BillingAddress { get; set; }
        public StorefrontOrderConfirmationCustomerDto Customer { get; set; }

        public decimal PaidPriceDecimal
        {
            get
            {
                return decimal.Round(PaidPrice.ToDecimal(), 3, MidpointRounding.AwayFromZero);
            }
        }

        public string InstallmentDescription
        {
            get
            {
                return string.Format("{0} X {1}", string.Format("{0} {1}", this.Installment, Resource.Installment), (this.PaidPriceDecimal / this.Installment.ToInt()).CurrencySign());
            }
        }
    }

    /// <summary>
    /// Line item row rendered in the order-items table: ProductName, Quantity, ProductSalePrice, TotalPrice.
    /// </summary>
    public class StorefrontOrderConfirmationItemDto
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal ProductSalePrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    /// <summary>
    /// Address block for confirmation pages: Name + Description + City/Country/ZipCode rendered,
    /// Street/District carried for EqualsAddress comparison.
    /// </summary>
    public class StorefrontOrderConfirmationAddressDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string Street { get; set; }
        public string District { get; set; }

        public bool EqualsAddress(StorefrontOrderConfirmationAddressDto other)
        {
            if (other == null)
            {
                return false;
            }

            return string.Equals(this.Street, other.Street, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.District, other.District, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.ZipCode, other.ZipCode, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.City, other.City, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.Country, other.Country, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Customer header block: FullName, Email, GsmNumber.
    /// </summary>
    public class StorefrontOrderConfirmationCustomerDto
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string GsmNumber { get; set; }

        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", Name.ToStr(), Surname.ToStr()).Trim();
            }
        }
    }
}
