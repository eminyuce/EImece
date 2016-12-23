using EImece.Domain.DbContext;
using EImece.Domain.Helpers;
using GenericRepository;
using GenericRepository.EntityFramework;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public abstract class BaseRepository<T, TId> : EntityRepository<T, TId>
       where T : class, IEntity<TId>
       where TId : IComparable
    {
        protected static readonly Logger BaseLogger = LogManager.GetCurrentClassLogger();

        protected IEImeceContext DbContext;
        protected EImeceContext EImeceDbContext
        {
            get
            {
                return (EImeceContext)DbContext;
            }
        }

        protected BaseRepository(IEImeceContext dbContext) : base(dbContext)
        {
            DbContext = dbContext;
            EImeceDbContext.Configuration.LazyLoadingEnabled = false;
            EImeceDbContext.Configuration.ProxyCreationEnabled = false;
            EImeceDbContext.Database.Log = s => BaseLogger.Trace(s);
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
        public virtual bool DeleteEntityByWhere(Expression<Func<T, bool>> whereLambda)
        {
            this.Delete(whereLambda);
            return true;
        }
        public virtual  EntitiesContext GetDbContext()
        {
            return EImeceDbContext;
        }
        public virtual int SaveOrEdit(T item)
        {
            if (item.Id.ToInt() == 0)
            {
                this.Add(item);
            }
            else
            {
                this.Edit(item);
            }

            return this.Save();
        }
        public virtual int DeleteItem(T item)
        {
            this.Delete(item);
            return this.Save();
        }




    }
}
