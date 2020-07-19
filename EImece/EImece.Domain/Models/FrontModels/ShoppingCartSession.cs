using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class ShoppingCartSession
    {
        private List<ShoppingCartItem> _shoppingCartItems = new List<ShoppingCartItem>();
        private Customer _customer = new Customer();
        private Address _shippingAddress = new Address();
        private Address _billingAddress = new Address();


        public List<ShoppingCartItem> ShoppingCartItems
        { 
            get {
                return _shoppingCartItems;
            }
            set {
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
            if(ShoppingCartItems.Any(r=>r.product.Id == item.product.Id))
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
                if (ShoppingCartItems.IsNullOrEmpty())
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
                return ShoppingCartItems.IsNullOrEmpty() ? 0 : ShoppingCartItems.Count;
            }
        }
    }
}
