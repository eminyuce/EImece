using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IShoppingCartRepository : IBaseEntityRepository<ShoppingCart>
    {
        List<ShoppingCart> GetAdminPageList(string search, int currentLanguage);

        Task<List<ShoppingCart>> GetAdminPageListAsync(string search, int currentLanguage, CancellationToken cancellationToken = default(CancellationToken));

        ShoppingCart GetShoppingCartByOrderGuid(string orderGuid);

        Task<ShoppingCart> GetShoppingCartByOrderGuidAsync(string orderGuid, CancellationToken cancellationToken = default(CancellationToken));

        int DeleteExpiredShoppingCarts(System.DateTime cutoffDate, int batchSize = 500);

        Task<int> DeleteExpiredShoppingCartsAsync(System.DateTime cutoffDate, int batchSize = 500, CancellationToken cancellationToken = default(CancellationToken));
    }
}