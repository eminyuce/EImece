using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
    }
}