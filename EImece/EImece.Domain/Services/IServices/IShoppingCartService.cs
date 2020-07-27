using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using Iyzipay.Model;

namespace EImece.Domain.Services.IServices
{
    public interface IShoppingCartService : IBaseEntityService<ShoppingCart>
    {
        ShoppingCart GetShoppingCartByOrderGuid(string orderGuid);

        void SaveOrEditShoppingCart(ShoppingCart item);

        void SaveShoppingCart(ShoppingCartSession shoppingCart, CheckoutForm checkoutForm);

        void DeleteByOrderGuid(string orderGuid);
    }
}