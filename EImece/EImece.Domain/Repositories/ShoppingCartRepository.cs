using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public class ShoppingCartRepository : BaseEntityRepository<ShoppingCart>, IShoppingCartRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ShoppingCartRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<ShoppingCart> GetAdminPageList(string search, int currentLanguage)
        {
            var items = GetAll().Where(r => r.Lang == currentLanguage);
            if (!string.IsNullOrEmpty(search))
            {
                items = items.Where(r => r.Name.Contains(search));
            }
            items = items.OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate);

            return items.ToList();
        }

        public ShoppingCart GetShoppingCartByOrderGuid(string orderGuid)
        {
            return FindBy(r => r.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public async Task<ShoppingCart> GetShoppingCartByOrderGuidAsync(string orderGuid, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await FindBy(r => r.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}