using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository.EntityFramework.Enums;
using Ninject;
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

        [Inject]
        public ICustomerRepository CustomerRepository { get; set; }

        public OrderRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }

        public Order GetOrderById(int id)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.OrderProducts);
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.MainImage));
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.ProductCategory));
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);

            Expression<Func<Order, bool>> match = r2 => r2.Id == id;
            Expression<Func<Order, int>> keySelector = t => t.Position;
            var orders = FindAllIncluding(match, keySelector, OrderByType.Ascending, null, null, includeProperties.ToArray());
            search = search.ToStr().Trim();
            if (!String.IsNullOrEmpty(search))
            {
                orders = orders.Where(r =>
                r.OrderGuid.Equals(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                 r.OrderNumber.Equals(search, StringComparison.InvariantCultureIgnoreCase)
                );
            }
            orders = orders.OrderByDescending(r => r.UpdatedDate);

            return orders.ToList();
        }

        public List<Order> GetOrdersUserId(string userId, string search)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.OrderProducts);
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.MainImage));
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.ProductCategory));
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);

            Expression<Func<Order, bool>> match = r2 => r2.UserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase);
            Expression<Func<Order, int>> keySelector = t => t.Position;
            var orders = FindAllIncluding(match, keySelector, OrderByType.Ascending, null, null, includeProperties.ToArray());
            search = search.ToStr().Trim();
            if (!String.IsNullOrEmpty(search))
            {
                orders = orders.Where(r =>
                r.OrderGuid.Equals(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                 r.OrderNumber.Equals(search, StringComparison.InvariantCultureIgnoreCase)
                );
            }
            orders = orders.OrderByDescending(r => r.UpdatedDate);

            return orders.ToList();
        }
    }
}