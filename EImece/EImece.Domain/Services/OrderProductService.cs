using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class OrderProductService : BaseService<OrderProduct>, IOrderProductService
    {
        private readonly IOrderProductRepository OrderProductRepository;
        private readonly ILogger<OrderProductService> _logger;

        public OrderProductService(IOrderProductRepository repository, ILogger<OrderProductService> logger) : base(repository)
        {
            OrderProductRepository = repository;
            _logger = logger;
        }

        public bool DeleteOrderProductsByOrderId(int id)
        {
            return OrderProductRepository.DeleteByWhereCondition(r => r.OrderId == id);
        }

        public async Task<bool> DeleteOrderProductsByOrderIdAsync(int id)
        {
            return await OrderProductRepository.DeleteByWhereConditionAsync(r => r.OrderId == id).ConfigureAwait(false);
        }
    }
}
