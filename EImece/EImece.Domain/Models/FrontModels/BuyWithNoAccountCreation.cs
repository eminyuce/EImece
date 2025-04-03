using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Models.FrontModels
{
    [Serializable]
    public class BuyWithNoAccountCreation
    {
        private Customer _customer = new Customer();
        public string OrderGuid { get; set; }
        private List<ShoppingCartItem> _shoppingCartItems = new List<ShoppingCartItem>();
        public Coupon Coupon { get; set; }
        public string CouponStr { get { return Coupon == null ? "" : Coupon.Name; } }
        public string UrlReferrer { get; set; }
        public string OrderComments { get; set; }
        public Address ShippingAddress { get; set; }

        public string CouponName
        {
            get
            {
                return Coupon == null ? "" : Coupon.Name;
            }
        }

        public string ConversationId
        {
            get
            {
                return string.Format("c-{0}-p-{1}",
                _customer.Id.ToString(),
                string.Join(",", ShoppingCartItems.Select(r => r.Product.Id).ToArray()));
            }
        }
        public Customer Customer
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
        public Setting CargoCompany { get; set; }

        [JsonIgnore]
        public Setting BasketMinTotalPriceForCargo { get; set; }

        [JsonIgnore]
        public Setting CargoPrice { get; set; }

        [JsonIgnore]
        public decimal CargoPriceValue
        {
            get
            {
                if (TotalPrice == 0)
                    return 0;
                else if (BasketMinTotalPriceForCargoInt > 0 && TotalPrice > BasketMinTotalPriceForCargoInt)
                    return 0;
                else if (CargoPrice!=null && CargoPrice.SettingValue.ToDecimal() > 0)
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
                result -= CalculateCouponDiscount(result);
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
                    return result * per;
                }
            }

            return 0;
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
