using EImece.Domain.DbContext;
using EImece.Domain.GenericRepository;
using EImece.Domain.GenericRepository.EntityFramework;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EImece.Domain.Repositories
{
    public abstract class BaseRepository<T> : EntityRepository<T, int>
       where T : class, IEntity<int>
    {
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
                    throw new InvalidOperationException("DeleteEntityByWhere failed.", ex);
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
                    throw new InvalidOperationException("DeleteEntityByWhereAsync failed.", ex);
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
                throw new InvalidOperationException("SaveOrEdit validation failed: " + errorMessage, ex);
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
                string errorMessage = ex.Message;
                foreach (var errors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in errors.ValidationErrors)
                    {
                        // get the error message
                        errorMessage += " " + validationError.PropertyName + " " + validationError.ErrorMessage + "  ";
                    }
                }
                throw new InvalidOperationException("SaveOrEditAsync validation failed: " + errorMessage, ex);
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
                    throw new InvalidOperationException("DeleteItem failed.", ex);
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
                throw new InvalidOperationException("DeleteBaseEntity validation failed: " + message, ex);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("DeleteBaseEntity failed for ids: " + String.Join(",", values), exception);
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
                throw new InvalidOperationException("DeleteBaseEntityAsync validation failed: " + message, ex);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("DeleteBaseEntityAsync failed for ids: " + String.Join(",", values), exception);
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