using EImece.Domain.Entities;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface IOrderProductService : IBaseService<OrderProduct>
    {
        bool DeleteOrderProductsByOrderId(int id);

        Task<bool> DeleteOrderProductsByOrderIdAsync(int id);
    }
}