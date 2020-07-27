using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Linq;

namespace EImece.Domain.Repositories
{
    public class ShoppingCartRepository : BaseEntityRepository<ShoppingCart>, IShoppingCartRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ShoppingCartRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public ShoppingCart GetShoppingCartByOrderGuid(string orderGuid)
        {
            return FindBy(r => r.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
    }
}