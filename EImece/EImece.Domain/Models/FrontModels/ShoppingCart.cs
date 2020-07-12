using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public class ShoppingCart
    {
        public List<ShoppingCartItem> shoppingCartItems = new List<ShoppingCartItem>();

        public void Add(ShoppingCartItem item)
        {
            if(shoppingCartItems.Any(r=>r.product.Id == item.product.Id))
            {
                ShoppingCartItem existingItem = shoppingCartItems.FirstOrDefault(r => r.product.Id == item.product.Id);
                existingItem.quantity += item.quantity;
            }
            else
            {
                shoppingCartItems.Add(item);
            }
           
        }
    }
}
