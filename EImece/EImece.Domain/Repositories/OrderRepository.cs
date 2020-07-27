using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EImece.Domain.Repositories
{
    public class OrderRepository : BaseEntityRepository<Order>, IOrderRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public OrderRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public List<Order> GetOrdersUserId(string userId, string search)
        {
            Expression<Func<Order, object>> includeProperty3 = r => r.ShippingAddress;
            Expression<Func<Order, object>> includeProperty2 = r => r.BillingAddress;
            Expression<Func<Order, object>>[] includeProperties = { includeProperty2, includeProperty3 };
            var products = GetAllIncluding(includeProperties);
            search = search.ToStr().Trim();
            if (!String.IsNullOrEmpty(search))
            {
                products = products.Where(r =>
                r.OrderGuid.Equals(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                 r.OrderNumber.Equals(search, StringComparison.InvariantCultureIgnoreCase)
                );
            }
            products = products.OrderByDescending(r => r.UpdatedDate);

            return products.ToList();
        }
    }
}