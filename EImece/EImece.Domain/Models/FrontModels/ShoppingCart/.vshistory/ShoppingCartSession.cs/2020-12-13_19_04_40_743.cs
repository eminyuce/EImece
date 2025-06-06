﻿using EImece.Domain.Entities;
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
        public string OrderGuid { get; set; }
        public string UrlReferrer { get; set; }
        public string OrderComments { get; set; }

        [JsonIgnore]
        public Setting CargoCompany { get; set; }

        [JsonIgnore]
        public Setting BasketMinTotalPriceForCargo { get; set; }

        [JsonIgnore]
        public Setting CargoPrice { get; set; }

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
                ShoppingCartItem existingItem = ShoppingCartItems.FirstOrDefault(r => r.Product.Id == item.Product.Id);
                existingItem.Quantity += item.Quantity;
                existingItem.Product.ProductSpecItems.AddRange(item.Product.ProductSpecItems);
                foreach (var ProductSpecItems in item.Product.ProductSpecItems)
                {
                    if (existingItem.Product.ProductSpecItems.Contains(ProductSpecItems))
                    {
                        var existingItemSpecs = existingItem.Product.ProductSpecItems.FirstOrDefault(r => r.Equals(ProductSpecItems));
                        ProductSpecItems.Quantity += existingItemSpecs.Quantity;
                        existingItem.Product.ProductSpecItems.Add(ProductSpecItems);
                        existingItem.Product.ProductSpecItems.Remove(ProductSpecItems);
                    }
                    else
                    {
                        existingItem.Product.ProductSpecItems.Add(ProductSpecItems);
                    }
                }
                
            }
            else
            {
                ShoppingCartItems.Add(item);
            }
        }

        [JsonIgnore]
        public double CargoPriceValue
        {
            get
            {
                if (TotalPrice == 0)
                    return 0;
                else if (TotalPrice < BasketMinTotalPriceForCargoInt)
                    return CargoPrice.SettingValue.ToDouble();
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

        public double TotalPriceWithCargoPrice
        {
            get
            {
                return TotalPrice + CargoPriceValue;
            }
        }

        public double TotalPrice
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

        public double SubTotalPrice
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
            var shippingAddress = new Domain.Entities.Address();
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