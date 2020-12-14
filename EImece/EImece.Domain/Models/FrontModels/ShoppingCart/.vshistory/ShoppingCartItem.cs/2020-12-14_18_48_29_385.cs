using EImece.Domain.Helpers.Extensions;
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

        public bool IsSameProduct(ShoppingCartItem item)
        {
            if(Product.Id == item.Product.Id)
            {
                if(Product.ProductSpecItems.IsNotEmpty() && item.ProductSpecItems.IsNotEmpty())
                {

                }
            }
        }
    }
}