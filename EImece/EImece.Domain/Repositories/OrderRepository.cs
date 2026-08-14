using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.DependencyInjection;
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
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product));
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product.MainImage));
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.ProductCategory));
            var item = GetSingleIncluding(id, includeProperties.ToArray());

            return item;
        }

        public async Task<Order> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product));
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product.MainImage));
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.ProductCategory));
            var item = await GetSingleIncludingAsync(id, cancellationToken, includeProperties.ToArray()).ConfigureAwait(false);

            return item;
        }

        public Order GetByOrderNumber(string orderNumber)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product));
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product.MainImage));
            Expression<Func<Order, bool>> match = r2 => r2.OrderNumber == orderNumber;
            Expression<Func<Order, int>> keySelector = t => t.Position;
            var orders = FindAllIncludingReadOnly(match, keySelector, OrderByType.Ascending, null, null, includeProperties.ToArray());
            return orders.FirstOrDefault();
        }

        public async Task<Order> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product));
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product.MainImage));
            Expression<Func<Order, bool>> match = r2 => r2.OrderNumber == orderNumber;
            Expression<Func<Order, int>> keySelector = t => t.Position;
            var orders = FindAllIncludingReadOnly(match, keySelector, OrderByType.Ascending, null, null, includeProperties.ToArray());
            return await orders.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public List<Order> GetOrdersUserId(string userId, string search)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.OrderProducts);
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.MainImage));
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);

            search = search.ToStr().Trim();
            Expression<Func<Order, bool>> match;
            if (string.IsNullOrEmpty(search))
            {
                match = r2 => r2.UserId == userId;
            }
            else
            {
                var term = search;
                match = r2 => r2.UserId == userId
                    && (r2.OrderGuid == term || r2.OrderNumber == term);
            }

            Expression<Func<Order, DateTime>> keySelector = t => t.UpdatedDate;
            var orders = FindAllIncludingReadOnly(match, keySelector, OrderByType.Descending, null, null, includeProperties.ToArray());
            return orders.ToList();
        }

        public async Task<List<Order>> GetOrdersUserIdAsync(string userId, string search, CancellationToken cancellationToken = default(CancellationToken))
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.OrderProducts);
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.MainImage));
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);

            // Push UserId (+ optional OrderNumber/OrderGuid) filtering into SQL so the IX_Orders_UserId*
            // indexes can seek and we never materialise every order for the user just to discard most.
            search = search.ToStr().Trim();
            Expression<Func<Order, bool>> match;
            if (string.IsNullOrEmpty(search))
            {
                match = r2 => r2.UserId == userId;
            }
            else
            {
                var term = search;
                match = r2 => r2.UserId == userId
                    && (r2.OrderGuid == term || r2.OrderNumber == term);
            }

            Expression<Func<Order, DateTime>> keySelector = t => t.UpdatedDate;
            var orders = FindAllIncludingReadOnly(match, keySelector, OrderByType.Descending, null, null, includeProperties.ToArray());
            return await orders.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}