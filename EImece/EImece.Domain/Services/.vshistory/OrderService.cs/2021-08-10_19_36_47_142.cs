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

        private readonly IOrderRepository OrderRepository;

        private readonly IOrderProductService OrderProductService;

        [Inject]
        private ICustomerService CustomerService;

        public OrderService(IOrderRepository repository, ICustomerService customerService, IOrderProductService orderProductService) : base(repository)
        {
            OrderRepository = repository;
            OrderProductService = orderProductService;
            this.CustomerService = customerService;
        }

        public void DeleteByUserId(string userId)
        {
            var orderObjs = OrderRepository.GetOrdersUserId(userId, "");
            foreach (var order in orderObjs)
            {
                OrderProductService.DeleteOrderProductsByOrderId(order.Id);
                DeleteEntity(order);
            }
        }

        public Order GetByOrderGuid(string orderGuid)
        {
            return OrderRepository.FindBy(r => r.OrderGuid.Equals(orderGuid, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }

        public Order GetOrderById(int id)
        {
            var item = OrderRepository.GetSingle(id);
            item.Customer = CustomerService.GetUserId(item.UserId);
            return item;
        }

        public List<Order> GetOrdersByUserId(string userId)
        {
            return OrderRepository.FindAll(r => r.UserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase), r => r.CreatedDate, GenericRepository.EntityFramework.Enums.OrderByType.Descending, null, null);
        }

        public List<Order> GetOrdersUserId(string userId, string search = "")
        {
            return OrderRepository.GetOrdersUserId(userId, search);
        }
    }
}