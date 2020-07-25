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
    }
}
