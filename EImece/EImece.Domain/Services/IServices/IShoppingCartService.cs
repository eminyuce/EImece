using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using Iyzipay.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IShoppingCartService : IBaseEntityService<ShoppingCart>
    {
        ShoppingCart GetShoppingCartByOrderGuid(string orderGuid);

        Task<ShoppingCart> GetShoppingCartByOrderGuidAsync(string orderGuid);

        void SaveOrEditShoppingCart(ShoppingCart item);

        Task SaveOrEditShoppingCartAsync(ShoppingCart item);

        Order SaveShoppingCart(string orderNumber, ShoppingCartSession shoppingCart, CheckoutForm checkoutForm, string userId);

        Task<Order> SaveShoppingCartAsync(string orderNumber, ShoppingCartSession shoppingCart, CheckoutForm checkoutForm, string userId);

        void DeleteByOrderGuid(string orderGuid);

        Task DeleteByOrderGuidAsync(string orderGuid);

        Order SaveBuyNow(BuyNowModel buyNowSession, CheckoutForm checkoutForm);

        Task<Order> SaveBuyNowAsync(BuyNowModel buyNowSession, CheckoutForm checkoutForm);

        Order SaveBuyWithNoAccountCreation(string orderNumber, BuyWithNoAccountCreation buyWithNoAccountCreation, CheckoutForm checkoutForm);

        Task<Order> SaveBuyWithNoAccountCreationAsync(string orderNumber, BuyWithNoAccountCreation buyWithNoAccountCreation, CheckoutForm checkoutForm);
        List<ShoppingCart> GetAdminPageList(string search, int currentLanguage);
    }
}