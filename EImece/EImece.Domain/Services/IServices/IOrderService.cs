using EImece.Domain.Entities;
using System.Collections.Generic;
using EImece.Domain.Models.DTOs;

namespace EImece.Domain.Services.IServices
{
    public interface IOrderService : IBaseEntityService<Order>
    {
        Order GetByOrderGuid(string orderGuid);

        Order GetByOrderNumber(string orderNumber);

        List<Order> GetOrdersUserId(string userId, string search = "");

        Order GetOrderById(int id);

        OrderDto GetOrderByIdDto(int id);

        void DeleteByUserId(string userId);

        List<Order> GetOrdersByUserId(string userId);

        List<OrderDto> GetOrdersByUserIdDto(string userId);

        List<OrderDto> GetOrdersUserIdDto(string userId, string search = "");
    }
}