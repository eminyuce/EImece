using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels.ShoppingCart;
using System;
using System.Linq;

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

        public decimal TotalPrice
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
            if (item == null)
            {
                return false;
            }
            if (Product.Id == item.Product.Id)
            {
                if (Product.ProductSpecItems.IsNotEmpty() && item.Product.ProductSpecItems.IsNotEmpty())
                {
                    return Product.ProductSpecItems.FirstOrDefault().Equals(item.Product.ProductSpecItems.FirstOrDefault());
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
}