using EImece.Domain.DbContext;
using EImece.Domain.Helpers;
using GenericRepository;
using GenericRepository.EntityFramework;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public abstract class BaseRepository<T> : EntityRepository<T, int>
       where T : class, IEntity<int> 
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
        //    EImeceDbContext.Database.Log = s => BaseLogger.Trace(s);

        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    DbContext.Dispose();
                }
            }
            this.disposed = true;
        }
        public virtual bool DeleteByWhereCondition(Expression<Func<T, bool>> whereLambda)
        {
            var isResult = false;
            using (var transactionResult = this.GetDbContext().Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                // Re-Initialise Repository
                try
                {
                    this.Delete(whereLambda);
                    isResult =  this.Save() == 1;
                    transactionResult.Commit();
                }
                catch (Exception ex)
                {
                    transactionResult.Rollback();
                    BaseLogger.Error(ex, "DeleteEntityByWhere");
                }
            }
            return isResult;
        }
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public virtual EntitiesContext GetDbContext()
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
            int r = 0;
            var isResult = false;
            using (var transactionResult = this.GetDbContext().Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                // Re-Initialise Repository
                try
                {
                    this.Delete(item);
                    r = this.Save();
                    isResult = r == 1;
                    transactionResult.Commit();
                }
                catch (Exception ex)
                {
                    transactionResult.Rollback();
                    isResult = false;
                    BaseLogger.Error(ex, "DeleteItem");
                }
            }
            return r;
        }
        public T[] ExecuteStoreQuery<T>(string commandText, params object[] parameters)
        {
            EntitiesContext objectContext = this.GetDbContext();
            DbRawSqlQuery<T> result = objectContext.Database.SqlQuery<T>(commandText, parameters);
            return result.ToArray();
        }
   

        public virtual void DeleteBaseEntity(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    var item = GetSingle(id);
                    Delete(item);
                }
                Save();
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                BaseLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                BaseLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public List<Expression<Func<T, object>>> GetIncludePropertyExpressionList<T>()
        {
            return new List<Expression<Func<T, object>>>();
        }

    }
}
