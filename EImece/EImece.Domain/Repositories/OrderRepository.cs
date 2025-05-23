﻿using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.GenericRepository.EntityFramework.Enums;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
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
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product.MainImage));
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.ProductCategory));
            var item = GetSingleIncluding(id, includeProperties.ToArray());

            return item;
        }

        public Order GetByOrderNumber(string orderNumber)
        {
            var includeProperties = GetIncludePropertyExpressionList();
            includeProperties.Add(r => r.ShippingAddress);
            includeProperties.Add(r => r.BillingAddress);
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product));
            includeProperties.Add(r => r.OrderProducts.Select(q => q.Product.MainImage));
            includeProperties.Add(r => r.OrderProducts.Select(r1 => r1.Product.ProductCategory));
            Expression<Func<Order, bool>> match = r2 => r2.OrderNumber.Equals(orderNumber, StringComparison.InvariantCultureIgnoreCase);
            Expression<Func<Order, int>> keySelector = t => t.Position;
            var orders = FindAllIncluding(match, keySelector, OrderByType.Ascending, null, null, includeProperties.ToArray());
            return orders.FirstOrDefault();
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