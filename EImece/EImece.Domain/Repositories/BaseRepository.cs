using EImece.Domain.DbContext;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using GenericRepository;
using GenericRepository.EntityFramework;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;

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
        }

        ~BaseRepository()
        {
            Dispose(false);
        }

        protected void Dispose(Boolean disposing)
        {
            // free unmanaged ressources here
            if (disposing)
            {
                // This method is called from Dispose() so it is safe to
                // free managed ressources here
                if (DbContext != null)
                {
                    DbContext.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
                    isResult = this.Save() == 1;
                    transactionResult.Commit();
                }
                catch (Exception ex)
                {
                    transactionResult.Rollback();
                    BaseLogger.Error(ex, "DeleteEntityByWhere");
                    throw;
                }
            }
            return isResult;
        }

        public virtual EntitiesContext GetDbContext()
        {
            return EImeceDbContext;
        }

        public virtual int SaveOrEdit(T item)
        {
            try
            {
                item.TrimAllStrings();
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
            catch (DbEntityValidationException ex)
            {
                string errorMessage = ex.Message;
                foreach (var errors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in errors.ValidationErrors)
                    {
                        // get the error message
                        errorMessage += " " + validationError.PropertyName + " " + validationError.ErrorMessage + "  ";
                    }
                }
                BaseLogger.Error(errorMessage);
                throw;
            }
        }

        public virtual int DeleteItem(T item)
        {
            int r = 0;
            using (var transactionResult = this.GetDbContext().Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                // Re-Initialise Repository
                try
                {
                    this.Delete(item);
                    r = this.Save();
                    transactionResult.Commit();
                }
                catch (Exception ex)
                {
                    transactionResult.Rollback();
                    BaseLogger.Error(ex, "DeleteItem");
                    throw;
                }
            }
            return r;
        }

        public T[] ExecuteStoreQuery(string commandText, params object[] parameters)
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
                throw;
            }
            catch (Exception exception)
            {
                BaseLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
                throw;
            }
        }

        public List<Expression<Func<T, object>>> GetIncludePropertyExpressionList()
        {
            return new List<Expression<Func<T, object>>>();
        }

        public List<Expression<Func<T, bool>>> GetWherePropertyExpressionList()
        {
            return new List<Expression<Func<T, bool>>>();
        }
    }
}