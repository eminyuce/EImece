using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.DTOs.Storefront;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Models.FrontModels
{
    [Serializable]
    public class BuyWithNoAccountCreation
    {
        private CustomerDto _customer = new CustomerDto();
        public string OrderGuid { get; set; }
        private List<ShoppingCartItem> _shoppingCartItems = new List<ShoppingCartItem>();
        public CouponDto Coupon { get; set; }
        public string CouponStr { get { return Coupon == null ? "" : Coupon.Code; } }
        public decimal CouponValidatedDiscount { get; set; }
        public decimal CouponShippingDiscount { get; set; }
        public decimal CouponEligibleAmount { get; set; }
        public string UrlReferrer { get; set; }
        public string OrderComments { get; set; }
        public AddressDto ShippingAddress { get; set; }

        public string CouponName
        {
            get
            {
                return Coupon == null ? "" : Coupon.Name;
            }
        }
        // we use ConversationId as OrderNumber in Order table
        // İstek esnasında gönderip, sonuçta alabileceğiniz bir değer,
        // request/response eşleşmesi yapmak için kullanılabilir.
        // En yaygın kullanış biçimi üye iş yerinin ürün numarasıdır.
        public string ConversationId
        {
            get
            {
                return GeneralHelper.RandomNumber(20);
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

        [JsonIgnore]
        public SettingValueDto CargoCompany { get; set; }

        [JsonIgnore]
        public SettingValueDto BasketMinTotalPriceForCargo { get; set; }

        [JsonIgnore]
        public SettingValueDto CargoPrice { get; set; }

        [JsonIgnore]
        public decimal CargoPriceValue
        {
            get
            {
                if (TotalPrice == 0)
                    return 0;
                else if (BasketMinTotalPriceForCargoInt > 0 && TotalPrice > BasketMinTotalPriceForCargoInt)
                    return 0;
                else if (CargoPrice != null && CargoPrice.SettingValue.ToDecimal() > 0)
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
                return BasketMinTotalPriceForCargo == null ? 0 : BasketMinTotalPriceForCargo.SettingValue.ToInt();
            }
        }

        public decimal TotalPriceWithCargoPrice
        {
            get
            {
                var result = TotalPrice + CargoPriceValue;
                if (Coupon != null)
                {
                    decimal couponDisc = CouponValidatedDiscount;
                    if (couponDisc == 0)
                    {
                        couponDisc = CalculateCouponDiscount(result);
                        if (Coupon.DiscountType == Models.Enums.CouponDiscountType.Percentage && Coupon.MaximumDiscountAmount.HasValue && couponDisc > Coupon.MaximumDiscountAmount.Value)
                            couponDisc = Coupon.MaximumDiscountAmount.Value;
                    }
                    result -= couponDisc;
                    result -= CouponShippingDiscount;
                }
                else
                {
                    result -= CalculateCouponDiscount(result);
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
                if (CouponValidatedDiscount > 0) return CouponValidatedDiscount;
                if (Coupon.IsFreeShipping) return 0;
                if (Coupon.DiscountType == Models.Enums.CouponDiscountType.Percentage && Coupon.DiscountPercentage > 0)
                {
                    decimal per = (decimal)Coupon.DiscountPercentage / 100;
                    var disc = result * per;
                    if (Coupon.MaximumDiscountAmount.HasValue && disc > Coupon.MaximumDiscountAmount.Value)
                        disc = Coupon.MaximumDiscountAmount.Value;
                    if (disc > result) disc = result;
                    return disc;
                }
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
    }
}