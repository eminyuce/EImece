using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IShoppingCartRepository : IBaseEntityRepository<ShoppingCart>
    {
        List<ShoppingCart> GetAdminPageList(string search, int currentLanguage);
        ShoppingCart GetShoppingCartByOrderGuid(string orderGuid);
    }
}