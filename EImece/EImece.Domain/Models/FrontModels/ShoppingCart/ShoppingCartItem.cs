using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    [Serializable]
    public class ShoppingCartItem
    {
        public string ShoppingCartItemId { set; get; }
        public int quantity { set; get; }
        public ShoppingCartProduct product { set; get; }

        public override string ToString()
        {
            return String.Format("quantity:%d product:%d", quantity, product.Id);
        }
        public double UnitPrice
        {
            get
            {
                return product.Price;
            }
        }
        public double TotalPrice
        {
            get
            {
                return product.Price * quantity;
            }
        }
        public string PName
        {
            get
            {
                return product.Name;
            }
        }
    }
}
