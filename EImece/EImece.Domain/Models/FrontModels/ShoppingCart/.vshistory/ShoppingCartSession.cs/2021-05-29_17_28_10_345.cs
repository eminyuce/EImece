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
    public class ShoppingCartSession
    {
        private List<ShoppingCartItem> _shoppingCartItems = new List<ShoppingCartItem>();
        private Customer _customer = new Customer();
        private Address _shippingAddress = new Address();
        private Address _billingAddress = new Address();
        public int CurrentLanguage { get; set; }

        public string OrderGuid { get; set; }
        public Coupon Coupon { get; set; }
        public string UrlReferrer { get; set; }
        public string OrderComments { get; set; }

        [JsonIgnore]
        public Setting CargoCompany { get; set; }

        [JsonIgnore]
        public Setting BasketMinTotalPriceForCargo { get; set; }

        [JsonIgnore]
        public Setting CargoPrice { get; set; }

        
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

        public string ConversationId
        {
            get
            {
                return string.Format("c-{0}-p-{1}",
                _customer.Id.ToString(),
                string.Join(",", ShoppingCartItems.Select(r => r.Product.Id).ToArray()));
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

        public Address ShippingAddress
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

        public Address BillingAddress
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
                else if (CargoPrice.SettingValue.ToDecimal() > 0)
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
                return BasketMinTotalPriceForCargo.SettingValue.ToInt();
            }
        }

        public decimal TotalPriceWithCargoPrice
        {
            get
            {
                var result = TotalPriceWithDiscount + CargoPriceValue;
                return CalculateCouponDiscount(result);
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
        public decimal TotalPriceWithDiscount
        {
            get
            {
                return TotalPrice-CalculateCouponDiscount(TotalPrice);
            }
        }

        public decimal CalculateCouponDiscount(decimal result)
        {
            if (Coupon != null)
            {
                if (Coupon.Discount > 0 && result > Coupon.Discount)
                {
                    return Coupon.Discount;
                }
                else if (Coupon.DiscountPercentage > 0)
                {
                    decimal per = (decimal)Coupon.DiscountPercentage / 100;
                    var couponDisc = result * per; 
                    var result2 = result - couponDisc;
                    if (result2 > 0)
                    {
                        return result2;
                    }
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

        public static ShoppingCartSession CreateDefaultShopingCard(int currentLanguage, string ip)
        {
            ShoppingCartSession shoppingCart = new ShoppingCartSession();
            var shippingAddress = new Address();
            shippingAddress.Country = "Turkiye";
            shippingAddress.AddressType = (int)AddressType.ShippingAddress;
            var billingAddress = new Domain.Entities.Address();
            billingAddress.Country = "Turkiye";
            billingAddress.AddressType = (int)AddressType.BillingAddress;
            shoppingCart.ShippingAddress = shippingAddress;
            shoppingCart.BillingAddress = billingAddress;
            Customer customer = shoppingCart.Customer;
            customer.IsSameAsShippingAddress = true;
            customer.Country = "Turkiye";
            customer.Ip = ip;
            customer.IdentityNumber = Guid.NewGuid().ToString();
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