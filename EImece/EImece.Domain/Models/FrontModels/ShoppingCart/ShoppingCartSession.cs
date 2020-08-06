using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.Enums;
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
            if (ShoppingCartItems.Any(r => r.product.Id == item.product.Id))
            {
                ShoppingCartItem existingItem = ShoppingCartItems.FirstOrDefault(r => r.product.Id == item.product.Id);
                existingItem.quantity += item.quantity;
            }
            else
            {
                ShoppingCartItems.Add(item);
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
                return ShoppingCartItems.Sum(r => r.product.Price * r.quantity);
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