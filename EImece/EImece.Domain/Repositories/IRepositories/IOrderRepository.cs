using EImece.Domain.Entities;
using System.Collections.Generic;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IOrderRepository : IBaseEntityRepository<Order>
    {
        List<Order> GetOrdersUserId(string userId, string search);

        Order GetOrderById(int id);
    }
}