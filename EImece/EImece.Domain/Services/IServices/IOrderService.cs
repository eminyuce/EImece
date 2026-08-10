using EImece.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IOrderService : IBaseEntityService<Order>
    {
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

        void DeleteByUserId(string userId);

        Task DeleteByUserIdAsync(string userId);

        List<Order> GetOrdersByUserId(string userId);

        Task<List<Order>> GetOrdersByUserIdAsync(string userId);
    }
}