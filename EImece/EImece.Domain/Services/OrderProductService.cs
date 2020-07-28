using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;

namespace EImece.Domain.Services
{
    public class OrderProductService : BaseService<OrderProduct>, IOrderProductService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IOrderProductRepository OrderProductRepository;

        public OrderProductService(IOrderProductRepository repository) : base(repository)
        {
            OrderProductRepository = repository;
        }

        public bool DeleteOrderProductsByOrderId(int id)
        {
            return OrderProductRepository.DeleteByWhereCondition(r => r.OrderId == id);
        }
    }
}