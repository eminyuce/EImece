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
     public class OrderProductService : BaseService<OrderProduct>, IOrderProductService
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IOrderProductRepository OrderProductRepository;

        public OrderProductService(IOrderProductRepository repository) : base(repository)
        {
            OrderProductRepository = repository;
        }
    }
}
