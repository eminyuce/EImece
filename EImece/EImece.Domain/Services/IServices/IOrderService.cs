using EImece.Domain.Entities;
using EImece.Domain.GenericRepository;
using EImece.Domain.Models.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IOrderService : IBaseEntityService<Order>
    {
        #region Storefront Read Methods (LINQ Projection, AsNoTracking)

        Task<OrderDto> GetStorefrontOrderByIdAsync(int orderId, CancellationToken cancellationToken = default(CancellationToken));
        OrderDto GetStorefrontOrderById(int orderId);

        Task<OrderDto> GetStorefrontOrderByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default(CancellationToken));
        OrderDto GetStorefrontOrderByOrderNumber(string orderNumber);

        Task<OrderDto> GetStorefrontOrderByGuidAsync(string orderGuid, CancellationToken cancellationToken = default(CancellationToken));
        OrderDto GetStorefrontOrderByGuid(string orderGuid);

        Task<List<OrderDto>> GetStorefrontOrdersByUserIdAsync(string userId, string search = "", CancellationToken cancellationToken = default(CancellationToken));
        List<OrderDto> GetStorefrontOrdersByUserId(string userId, string search = "");
        Task<List<Models.DTOs.Storefront.OrderListItemDto>> GetStorefrontOrderListByUserIdAsync(string userId, string search = "", CancellationToken cancellationToken = default(CancellationToken));
        List<Models.DTOs.Storefront.OrderListItemDto> GetStorefrontOrderListByUserId(string userId, string search = "");

        #endregion

        #region Admin / Change-Tracking Methods (Full Entities)

        Order GetByOrderGuid(string orderGuid);

        Task<Order> GetByOrderGuidAsync(string orderGuid);

        Order GetByPaymentId(string paymentId);

        Task<Order> GetByPaymentIdAsync(string paymentId);

        Order GetByOrderNumber(string orderNumber);

        Task<Order> GetByOrderNumberAsync(string orderNumber);

        List<Order> GetOrdersUserId(string userId, string search = "");

        Task<List<Order>> GetOrdersUserIdAsync(string userId, string search = "");

        Order GetOrderById(int id);

        Task<Order> GetOrderByIdAsync(int id);

        void DeleteOrderById(int id);

        Task DeleteOrderByIdAsync(int id);

        void DeleteByUserId(string userId);

        Task DeleteByUserIdAsync(string userId);

        List<Order> GetOrdersByUserId(string userId);

        Task<List<Order>> GetOrdersByUserIdAsync(string userId);

        #endregion
    }
}