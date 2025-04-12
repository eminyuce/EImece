using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using Iyzipay.Model;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface IShoppingCartService : IBaseEntityService<ShoppingCart>
    {
        ShoppingCart GetShoppingCartByOrderGuid(string orderGuid);

        void SaveOrEditShoppingCart(ShoppingCart item);

        Order SaveShoppingCart(string orderNumber, ShoppingCartSession shoppingCart, CheckoutForm checkoutForm, string userId);

        void DeleteByOrderGuid(string orderGuid);

        Order SaveBuyNow(BuyNowModel buyNowSession, CheckoutForm checkoutForm);

        Order SaveBuyWithNoAccountCreation(string orderNumber, BuyWithNoAccountCreation buyWithNoAccountCreation, CheckoutForm checkoutForm);
        List<ShoppingCart> GetAdminPageList(string search, int currentLanguage);
    }
}