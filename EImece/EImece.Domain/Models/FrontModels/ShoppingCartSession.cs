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
        public List<ShoppingCartItem> ShoppingCartItems = new List<ShoppingCartItem>();
        public Customer Customer = new Customer();
        public Address ShippingAddress = new Address();
        public Address BillingAddress = new Address();

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
    }
}
