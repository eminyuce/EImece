using EImece.Domain.DbContext;
using EImece.Domain.GenericRepository;
using EImece.Domain.GenericRepository.EntityFramework;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        }

        /// <summary>
        /// Lifetime of DbContext is managed exclusively by the DI container (Scoped per request).
        /// Repositories must not dispose the injected shared DbContext.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            // No-op: DbContext lifetime is owned and disposed by Microsoft.Extensions.DependencyInjection request scope.
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

        public virtual async Task<bool> DeleteByWhereConditionAsync(Expression<Func<T, bool>> whereLambda)
        {
            var isResult = false;
            using (var transactionResult = this.GetDbContext().Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                try
                {
                    var objects = await FindBy(whereLambda).ToListAsync().ConfigureAwait(false);
                    foreach (var obj in objects)
                    {
                        GetDbContext().Set<T>().Remove(obj);
                    }
                    isResult = await this.SaveAsync().ConfigureAwait(false) == 1;
                    transactionResult.Commit();
                }
                catch (Exception ex)
                {
                    transactionResult.Rollback();
                    BaseLogger.Error(ex, "DeleteEntityByWhere");
                    throw new InvalidOperationException("DeleteByWhereConditionAsync failed.", ex);
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
                BaseLogger.Error(ex, BuildEntityValidationErrorMessage(ex));
                throw;
            }
        }

        public virtual async Task<int> SaveOrEditAsync(T item)
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

                return await this.SaveAsync().ConfigureAwait(false);
            }
            catch (DbEntityValidationException ex)
            {
                BaseLogger.Error(ex, BuildEntityValidationErrorMessage(ex));
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

        public virtual async Task<int> DeleteItemAsync(T item)
        {
            int r = 0;
            using (var transactionResult = this.GetDbContext().Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                try
                {
                    this.Delete(item);
                    r = await this.SaveAsync().ConfigureAwait(false);
                    transactionResult.Commit();
                }
                catch (Exception ex)
                {
                    transactionResult.Rollback();
                    BaseLogger.Error(ex, "DeleteItem");
                    throw new InvalidOperationException("DeleteItemAsync failed.", ex);
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

        public virtual async Task DeleteBaseEntityAsync(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    var item = await GetSingleAsync(id).ConfigureAwait(false);
                    Delete(item);
                }
                await SaveAsync().ConfigureAwait(false);
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                BaseLogger.Error(ex, "DbEntityValidationException:" + message);
                throw new InvalidOperationException("DeleteBaseEntityAsync validation failed.", ex);
            }
            catch (Exception exception)
            {
                BaseLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
                throw new InvalidOperationException("DeleteBaseEntityAsync failed.", exception);
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

        private static string BuildEntityValidationErrorMessage(DbEntityValidationException ex)
        {
            var errorMessage = new StringBuilder(ex.Message);
            foreach (var errors in ex.EntityValidationErrors)
            {
                foreach (var validationError in errors.ValidationErrors)
                {
                    errorMessage.Append(" ")
                        .Append(validationError.PropertyName)
                        .Append(" ")
                        .Append(validationError.ErrorMessage)
                        .Append("  ");
                }
            }
            return errorMessage.ToString();
        }
    }
}