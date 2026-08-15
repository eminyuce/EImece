using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Models.Payment;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IShoppingCartService : IBaseEntityService<ShoppingCart>
    {
        ShoppingCart GetShoppingCartByOrderGuid(string orderGuid);

        Task<ShoppingCart> GetShoppingCartByOrderGuidAsync(string orderGuid);

        void SaveOrEditShoppingCart(ShoppingCart item);

        Task SaveOrEditShoppingCartAsync(ShoppingCart item);

        Order SaveShoppingCart(string orderNumber, ShoppingCartSession shoppingCart, PaymentResult paymentResult, string userId);

        Task<Order> SaveShoppingCartAsync(string orderNumber, ShoppingCartSession shoppingCart, PaymentResult paymentResult, string userId);

        void DeleteByOrderGuid(string orderGuid);

        Task DeleteByOrderGuidAsync(string orderGuid);

        Order SaveBuyNow(BuyNowModel buyNowSession, PaymentResult paymentResult);

        Task<Order> SaveBuyNowAsync(BuyNowModel buyNowSession, PaymentResult paymentResult);

        Order SaveBuyWithNoAccountCreation(string orderNumber, BuyWithNoAccountCreation buyWithNoAccountCreation, PaymentResult paymentResult);

        Task<Order> SaveBuyWithNoAccountCreationAsync(string orderNumber, BuyWithNoAccountCreation buyWithNoAccountCreation, PaymentResult paymentResult);
        List<ShoppingCart> GetAdminPageList(string search, int currentLanguage);

        Task<List<ShoppingCart>> GetAdminPageListAsync(string search, int currentLanguage, CancellationToken cancellationToken = default(CancellationToken));

        int ClearExpiredShoppingCarts(int olderThanDays = 30);

        Task<int> ClearExpiredShoppingCartsAsync(int olderThanDays = 30, CancellationToken cancellationToken = default(CancellationToken));
    }
}