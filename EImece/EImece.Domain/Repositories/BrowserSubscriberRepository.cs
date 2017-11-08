using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using NLog;

namespace EImece.Domain.Repositories
{
    public class BrowserSubscriberRepository : BaseEntityRepository<BrowserSubscriber>, IBrowserSubscriberRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public BrowserSubscriberRepository(IEImeceContext dbContext) : base(dbContext)
        {

        }
    }
}
