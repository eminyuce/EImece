using EImece.Domain.Entities;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IOrderRepository : IBaseEntityRepository<Order>, IDisposable
    {
        List<Order> GetOrdersUserId(string userId, string search);
    }
}