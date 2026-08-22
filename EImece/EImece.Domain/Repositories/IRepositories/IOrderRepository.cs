using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
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

        #region Storefront Read Methods (LINQ Projection, AsNoTracking)

        Task<OrderDto> GetStorefrontOrderByIdAsync(int id, CancellationToken cancellationToken = default(CancellationToken));
        OrderDto GetStorefrontOrderById(int id);

        Task<OrderDto> GetStorefrontOrderByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default(CancellationToken));
        OrderDto GetStorefrontOrderByOrderNumber(string orderNumber);

        Task<OrderDto> GetStorefrontOrderByGuidAsync(string orderGuid, CancellationToken cancellationToken = default(CancellationToken));
        OrderDto GetStorefrontOrderByGuid(string orderGuid);

        Task<List<OrderDto>> GetStorefrontOrdersByUserIdAsync(string userId, string search, CancellationToken cancellationToken = default(CancellationToken));
        List<OrderDto> GetStorefrontOrdersByUserId(string userId, string search);
        Task<List<Models.DTOs.Storefront.OrderListItemDto>> GetStorefrontOrderListByUserIdAsync(string userId, string search, CancellationToken cancellationToken = default(CancellationToken));
        List<Models.DTOs.Storefront.OrderListItemDto> GetStorefrontOrderListByUserId(string userId, string search);

        #endregion
    }
}