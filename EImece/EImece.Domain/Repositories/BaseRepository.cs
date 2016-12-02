using EImece.Domain.DbContext;
using GenericRepository;
using GenericRepository.EntityFramework;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public abstract class BaseRepository<T, TId> : EntityRepository<T, TId>
       where T : class, IEntity<TId>
       where TId : IComparable
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected IEImeceContext DbContext;
        protected EImeceContext StoreDbContext
        {
            get
            {
                return (EImeceContext)DbContext;
            }
        }

        protected BaseRepository(IEImeceContext dbContext) : base(dbContext)
        {
            DbContext = dbContext;
            StoreDbContext.Configuration.LazyLoadingEnabled = false;
            StoreDbContext.Database.Log = s => Logger.Trace(s);
        }
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.DbContext.Dispose();
                }
            }
            this.disposed = true;
        }
    }
}
