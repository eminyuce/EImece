using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using GenericRepository;
using System.Data.Entity;
using SharkDev.Web.Controls.TreeView.Model;
using EImece.Domain.Helpers;
using System.Linq.Expressions;
using NLog;

namespace EImece.Domain.Repositories
{
    public class BrowserSubscriptionRepository : BaseEntityRepository<BrowserSubscription>, IBrowserSubscriptionRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BrowserSubscriptionRepository(IEImeceContext dbContext) : base(dbContext)
        {

        }
    }
}
