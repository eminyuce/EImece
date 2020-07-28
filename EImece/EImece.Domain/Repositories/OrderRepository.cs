using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using Microsoft.Owin.Security.OAuth;
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
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product));
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product));
            var item = GetSingleIncluding(id, includeProperties.ToArray());

            return item;
        }
        public List<Order> GetOrdersUserId(string userId, string search)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.OrderProducts);

            var products = GetAllIncluding(includeProperties.ToArray());
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