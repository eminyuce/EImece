using EImece.Domain.Entities;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IShoppingCartRepository : IBaseEntityRepository<ShoppingCart>
    {
        ShoppingCart GetShoppingCartByOrderGuid(string orderGuid);
    }
}