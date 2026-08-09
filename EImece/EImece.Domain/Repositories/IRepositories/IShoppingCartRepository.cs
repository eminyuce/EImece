using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IShoppingCartRepository : IBaseEntityRepository<ShoppingCart>
    {
        List<ShoppingCart> GetAdminPageList(string search, int currentLanguage);
        ShoppingCart GetShoppingCartByOrderGuid(string orderGuid);

        Task<ShoppingCart> GetShoppingCartByOrderGuidAsync(string orderGuid, CancellationToken cancellationToken = default(CancellationToken));
    }
}