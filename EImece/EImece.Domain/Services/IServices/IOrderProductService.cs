using EImece.Domain.Entities;

namespace EImece.Domain.Services.IServices
{
    public interface IOrderProductService : IBaseService<OrderProduct>
    {
        bool DeleteOrderProductsByOrderId(int id);
    }
}