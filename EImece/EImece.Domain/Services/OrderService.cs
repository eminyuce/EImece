using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class OrderService : BaseEntityService<Order>, IOrderService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IOrderRepository OrderRepository;

        public OrderService(IOrderRepository repository) : base(repository)
        {
            OrderRepository = repository;
        }

        public Order GetByOrderGuid(string orderGuid)
        {
            return OrderRepository.FindBy(r => r.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public List<Order> GetOrdersUserId(string userId)
        {
            return OrderRepository.FindBy(r => r.UserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public List<Order> GetOrdersUserId(string userId, string search)
        {
            if (String.IsNullOrEmpty(search))
            {
                return GetOrdersUserId(userId);
            }
            else
            {
                return OrderRepository.FindBy(r =>
                r.UserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase)
                &&  r.OrderGuid.Equals(search, StringComparison.InvariantCultureIgnoreCase)
                ).ToList();
            }
        }
    }
}
