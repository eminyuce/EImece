using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IOrderService : IBaseEntityService<Order>
    {
        List<Order> GetOrdersUserId(string userId);
        Order GetByOrderGuid(string orderGuid);
        List<Order> GetOrdersUserId(string userId, string search);
    }
}
