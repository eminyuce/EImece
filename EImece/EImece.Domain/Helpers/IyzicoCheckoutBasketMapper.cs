using EImece.Domain.Models.FrontModels;
using Iyzipay.Model;
using Iyzipay.Request;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Maps cart lines onto Iyzico Checkout Form price / basketItems.
    /// Iyzico has no quantity field: each line Price must be unit * qty,
    /// and request.Price must equal the sum of those line prices.
    /// paidPrice is the charged total (items + cargo − coupon).
    /// </summary>
    public static class IyzicoCheckoutBasketMapper
    {
        public static void ApplyCart(CreateCheckoutFormInitializeRequest request, ShoppingCartSession shoppingCart)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (shoppingCart == null)
            {
                throw new ArgumentNullException(nameof(shoppingCart));
            }

            var basketItems = new List<BasketItem>();
            decimal basketTotal = 0m;

            foreach (var shoppingCartItem in shoppingCart.ShoppingCartItems)
            {
                var product = shoppingCartItem.Product;
                var quantity = shoppingCartItem.Quantity < 1 ? 1 : shoppingCartItem.Quantity;
                var lineTotal = CurrencyHelper.RoundPriceNumber(product.Price * quantity);
                basketItems.Add(new BasketItem
                {
                    Id = product.ProductCode,
                    Name = product.Name,
                    Category1 = product.CategoryName,
                    Category2 = AppConfig.ShoppingCartItemCategory2,
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = CurrencyHelper.CurrencySignForIyizo(lineTotal)
                });
                basketTotal += lineTotal;
            }

            request.Price = CurrencyHelper.CurrencySignForIyizo(basketTotal);
            request.PaidPrice = CurrencyHelper.CurrencySignForIyizo(shoppingCart.TotalPriceWithCargoPrice);
            request.BasketItems = basketItems;
        }
    }
}
