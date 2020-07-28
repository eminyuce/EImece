using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Services
{
    public class OrderService : BaseEntityService<Order>, IOrderService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IOrderRepository OrderRepository;

        [Inject]
        ICustomerService CustomerService;

        public OrderService(IOrderRepository repository, ICustomerService customerService) : base(repository)
        {
            OrderRepository = repository;
            this.CustomerService = customerService;
        }

        public Order GetByOrderGuid(string orderGuid)
        {
            return OrderRepository.FindBy(r => r.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public Order GetOrderById(int id)
        {
            var item = OrderRepository.GetOrderById(id);
            item.Customer = CustomerService.GetUserId(item.UserId);
            return item;
        }

        public List<Order> GetOrdersUserId(string userId, string search = "")
        {
            return OrderRepository.GetOrdersUserId(userId, search);
        }
    }
}