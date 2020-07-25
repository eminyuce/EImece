using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
   public class OrderRepository : BaseEntityRepository<Order>, IOrderRepository
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public OrderRepository(IEImeceContext dbContext) : base(dbContext)
        {

        }

    }
}
