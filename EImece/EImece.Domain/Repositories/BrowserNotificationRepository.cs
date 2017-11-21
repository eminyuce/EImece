using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using NLog;

namespace EImece.Domain.Repositories
{
    public class BrowserNotificationRepository : BaseEntityRepository<BrowserNotification>, IBrowserNotificationRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BrowserNotificationRepository(IEImeceContext dbContext) : base(dbContext)
        {
        }
    }
}
