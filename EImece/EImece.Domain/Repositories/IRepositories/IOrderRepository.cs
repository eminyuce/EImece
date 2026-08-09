using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories.IRepositories
{
    public interface IOrderRepository : IBaseEntityRepository<Order>
    {
        List<Order> GetOrdersUserId(string userId, string search);

        Task<List<Order>> GetOrdersUserIdAsync(string userId, string search, CancellationToken cancellationToken = default(CancellationToken));

        Order GetOrderById(int id);

        Task<Order> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken));

        Order GetByOrderNumber(string orderNumber);

        Task<Order> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default(CancellationToken));
    }
}