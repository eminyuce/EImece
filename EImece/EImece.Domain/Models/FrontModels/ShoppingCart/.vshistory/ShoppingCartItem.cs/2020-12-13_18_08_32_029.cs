using EImece.Domain.Models.FrontModels.ShoppingCart;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    [Serializable]
    public class ShoppingCartItem
    {
        public string ShoppingCartItemId { set; get; }
        public int Quantity { set; get; }
        public ShoppingCartProduct Product { set; get; }
        public List<ProductSpecItem> ProductSpecItems { set; get; } = new List<ProductSpecItem>();

        public override string ToString()
        {
            return String.Format("quantity:%d product:%d", Quantity, Product.Id);
        }

        public double UnitPrice
        {
            get
            {
                return Product.Price;
            }
        }

        public double TotalPrice
        {
            get
            {
                return Product.Price * Quantity;
            }
        }

        public string PName
        {
            get
            {
                return Product.Name;
            }
        }
    }
}