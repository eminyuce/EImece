using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Models.FrontModels
{
    [Serializable]
    public class ShoppingCartSession
    {
        private List<ShoppingCartItem> _shoppingCartItems = new List<ShoppingCartItem>();
        private CustomerDto _customer = new CustomerDto();
        private AddressDto _shippingAddress = new AddressDto();
        private AddressDto _billingAddress = new AddressDto();
        public int CurrentLanguage { get; set; }

        public string OrderGuid { get; set; }
        public CouponDto Coupon { get; set; }
        // Validated coupon amounts persisted with cart (set by CouponValidationService)
        public decimal CouponValidatedDiscount { get; set; }
        public decimal CouponShippingDiscount { get; set; }
        public decimal CouponEligibleAmount { get; set; }
        public string UrlReferrer { get; set; }
        public string OrderComments { get; set; }

        [JsonIgnore]
        public SettingValueDto CargoCompany { get; set; }

        [JsonIgnore]
        public SettingValueDto BasketMinTotalPriceForCargo { get; set; }

        [JsonIgnore]
        public SettingValueDto CargoPrice { get; set; }

        public string CouponCode
        {
            get
            {
                return Coupon == null ? "" : Coupon.Code;
            }
        }

        public string CouponName
        {
            get
            {
                return Coupon == null ? "" : Coupon.Name;
            }
        }

        public List<ShoppingCartItem> ShoppingCartItems
        {
            get
            {
                return _shoppingCartItems;
            }
            set
            {
                _shoppingCartItems = value;
            }
        }

        public CustomerDto Customer
        {
            get
            {
                return _customer;
            }
            set
            {
                _customer = value;
            }
        }

        public AddressDto ShippingAddress
        {
            get
            {
                return _shippingAddress;
            }
            set
            {
                _shippingAddress = value;
            }
        }

        public AddressDto BillingAddress
        {
            get
            {
                return _billingAddress;
            }
            set
            {
                _billingAddress = value;
            }
        }

        public void Add(ShoppingCartItem item)
        {
            if (ShoppingCartItems.Any(r => r.Product.Id == item.Product.Id))
            {
                var existingItem = ShoppingCartItems.FirstOrDefault(r => r.IsSameProduct(item));
                if (existingItem == null)
                {
                    ShoppingCartItems.Add(item);
                }
                else
                {
                    if (item.Product.ProductSpecItems.IsNotEmpty())
                    {
                        foreach (var ProductSpecItem in item.Product.ProductSpecItems)
                        {
                            if (existingItem.Product.ProductSpecItems.Contains(ProductSpecItem))
                            {
                                var existingItemSpecs = existingItem.Product.ProductSpecItems.FirstOrDefault(r => r.Equals(ProductSpecItem));
                                existingItem.Quantity += item.Quantity;
                            }
                        }
                    }
                    else
                    {
                        existingItem = ShoppingCartItems.FirstOrDefault(r => r.Product.Id == item.Product.Id);
                        existingItem.Quantity += item.Quantity;
                    }
                }
            }
            else
            {
                ShoppingCartItems.Add(item);
            }
        }

        [JsonIgnore]
        public decimal CargoPriceValue
        {
            get
            {
                if (TotalPrice == 0)
                    return 0;
                else if (BasketMinTotalPriceForCargoInt > 0 && TotalPrice > BasketMinTotalPriceForCargoInt)
                    return 0;
                else if (CargoPrice !=null && CargoPrice.SettingValue.ToDecimal() > 0)
                    return CargoPrice.SettingValue.ToDecimal();
                else
                    return 0;
            }
        }

        [JsonIgnore]
        public int BasketMinTotalPriceForCargoInt
        {
            get
            {
                if(BasketMinTotalPriceForCargo == null)
                {
                    return 0;
                }
                return BasketMinTotalPriceForCargo.SettingValue.ToInt();
            }
        }

        public decimal TotalPriceWithCargoPrice
        {
            get
            {
                var result = TotalPrice + CargoPriceValue;
                if (Coupon != null)
                {
                    // Prefer validated discount (covers max cap, eligible-only, free shipping via separate field)
                    decimal couponDisc = CouponValidatedDiscount;
                    if (couponDisc == 0)
                    {
                        // Backward compat fallback when validation not yet run (e.g., legacy cart)
                        // For free-shipping coupons without validated amount, fallback also checks IsFreeShipping
                        if (Coupon.IsFreeShipping)
                        {
                            couponDisc = 0;
                        }
                        else
                        {
                            // Use eligible-aware fallback? Old logic used total price; keep until revalidation updates validated fields
                            couponDisc = CalculateCouponDiscount(result);
                            // For percentage with max cap, apply cap if present
                            if (Coupon.DiscountType == CouponDiscountType.Percentage && Coupon.MaximumDiscountAmount.HasValue && Coupon.MaximumDiscountAmount.Value > 0 && couponDisc > Coupon.MaximumDiscountAmount.Value)
                                couponDisc = Coupon.MaximumDiscountAmount.Value;
                        }
                    }
                    result -= couponDisc;
                    result -= CouponShippingDiscount;
                }
                if (result < 0)
                {
                    return 0;
                }
                return result;
            }
        }

        public decimal TotalPrice
        {
            get
            {
                if (ShoppingCartItems.IsEmpty())
                {
                    return 0;
                }
                return ShoppingCartItems.Sum(r => r.Product.Price * r.Quantity);
            }
        }

        public decimal CalculateCouponDiscount(decimal result)
        {
            if (Coupon != null)
            {
                // Use validated amount if already computed (covers product restrictions, max cap)
                if (CouponValidatedDiscount > 0) return CouponValidatedDiscount;
                if (Coupon.IsFreeShipping) return 0;
                // Respect DiscountType
                if (Coupon.DiscountType == CouponDiscountType.Percentage && Coupon.DiscountPercentage > 0)
                {
                    decimal per = (decimal)Coupon.DiscountPercentage / 100;
                    var disc = result * per;
                    if (Coupon.MaximumDiscountAmount.HasValue && Coupon.MaximumDiscountAmount.Value > 0 && disc > Coupon.MaximumDiscountAmount.Value)
                        disc = Coupon.MaximumDiscountAmount.Value;
                    // Also cap to result (never negative)
                    if (disc > result) disc = result;
                    return disc;
                }
                if (Coupon.DiscountType == CouponDiscountType.FixedAmount || Coupon.Discount > 0)
                {
                    if (Coupon.Discount > 0)
                    {
                        if (result >= Coupon.Discount)
                        {
                            return Coupon.Discount;
                        }
                        else
                        {
                            return result;
                        }
                    }
                }
                else if (Coupon.DiscountPercentage > 0)
                {
                    decimal per = (decimal)Coupon.DiscountPercentage / 100;
                    var disc = result * per;
                    if (Coupon.MaximumDiscountAmount.HasValue && disc > Coupon.MaximumDiscountAmount.Value)
                        disc = Coupon.MaximumDiscountAmount.Value;
                    return disc;
                }
            }

            return 0;
        }

        public void SetValidatedCouponDiscount(decimal discount, decimal shippingDiscount, decimal eligibleAmount)
        {
            CouponValidatedDiscount = discount;
            CouponShippingDiscount = shippingDiscount;
            CouponEligibleAmount = eligibleAmount;
        }

        public void ClearValidatedCoupon()
        {
            Coupon = null;
            CouponValidatedDiscount = 0;
            CouponShippingDiscount = 0;
            CouponEligibleAmount = 0;
        }

        public decimal SubTotalPrice
        {
            get
            {
                return TotalPrice;
            }
        }

        public int TotalItemCount
        {
            get
            {
                return ShoppingCartItems.IsEmpty() ? 0 : ShoppingCartItems.Count;
            }
        }

        public static ShoppingCartSession CreateDefaultShopingCard(int currentLanguage, string ip)
        {
            var shoppingCart = new ShoppingCartSession();
            var shippingAddress = new AddressDto();
            shippingAddress.Country = Constants.IYZICO_ADDRESS_COUNTRY;
            shippingAddress.AddressType = (int)AddressType.ShippingAddress;
            var billingAddress = new AddressDto();
            billingAddress.Country = Constants.IYZICO_ADDRESS_COUNTRY;
            billingAddress.AddressType = (int)AddressType.BillingAddress;
            shoppingCart.ShippingAddress = shippingAddress;
            shoppingCart.BillingAddress = billingAddress;
            CustomerDto customer = shoppingCart.Customer;
            customer.IsSameAsShippingAddress = true;
            customer.Country = Constants.IYZICO_ADDRESS_COUNTRY;
            customer.Ip = ip;
            customer.IdentityNumber = "";
            customer.CreatedDate = DateTime.Now;
            customer.UpdatedDate = DateTime.Now;
            customer.IsActive = true;
            customer.Position = 1;
            customer.Lang = currentLanguage;
            shoppingCart.Customer = customer;

            return shoppingCart;
        }
    }
}