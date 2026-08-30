using AutoMapper;
using EImece.Domain.Caching;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.GenericRepository;
using EImece.Domain.Helpers;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public abstract class BaseService<T> where T : class, IEntity<int>
    {
        /// <summary>
        /// Async cached reads deliberately use their own cache keys. LazyCache stores a
        /// <c>Lazy&lt;T&gt;</c> for GetOrAdd and an <c>AsyncLazy&lt;T&gt;</c> for GetOrAddAsync, and if a
        /// synchronous reader lands on an entry that an async reader created, LazyCache unwraps it
        /// with GetAwaiter().GetResult() internally. Separate keys keep the sync and async paths
        /// from ever blocking on each other while both spellings of a method still exist.
        /// </summary>
        protected const string AsyncCacheKeySuffix = "-async";

        public bool IsCachingActivated { get; set; } = true;

        private readonly IBaseRepository<T> baseRepository;

        protected BaseService(IBaseRepository<T> baseRepository)
        {
            this.baseRepository = baseRepository ?? throw new ArgumentNullException(nameof(baseRepository));
        }

        protected BaseService(IBaseRepository<T> baseRepository, bool isCachingActivated)
        {
            this.baseRepository = baseRepository ?? throw new ArgumentNullException(nameof(baseRepository));
            this.IsCachingActivated = isCachingActivated;
        }

        public virtual List<T> LoadEntites(Expression<Func<T, bool>> whereLambda)
        {
            return baseRepository.FindBy(whereLambda).ToList();
        }

        public virtual List<T> GetAll()
        {
            return baseRepository.GetAll().ToList();
        }

        public virtual T GetSingle(int id)
        {
            return baseRepository.GetSingle(id);
        }

        public virtual T SaveOrEditEntity(T entity)
        {
            var tmp = baseRepository.SaveOrEdit(entity);
            return entity;
        }

        public virtual bool DeleteEntity(T entity)
        {
            var result = this.baseRepository.DeleteItem(entity);
            return result > 0;
        }

        public virtual async Task<bool> DeleteEntityAsync(T entity)
        {
            var result = await this.baseRepository.DeleteItemAsync(entity).ConfigureAwait(false);
            return result > 0;
        }

        public virtual bool DeleteById(int id)
        {
            return this.baseRepository.DeleteByWhereCondition(r => r.Id == id);
        }

        public virtual void DeleteBaseEntity(List<string> values)
        {
            baseRepository.DeleteBaseEntity(values);
        }

        public virtual async Task DeleteBaseEntityAsync(List<string> values)
        {
            await baseRepository.DeleteBaseEntityAsync(values).ConfigureAwait(false);
        }

        public virtual async Task<bool> DeleteByIdAsync(int id)
        {
            return await this.baseRepository.DeleteByWhereConditionAsync(r => r.Id == id).ConfigureAwait(false);
        }

        public virtual async Task<T> GetSingleAsync(int id)
        {
            return await baseRepository.GetSingleAsync(id).ConfigureAwait(false);
        }

        public virtual async Task<T> SaveOrEditEntityAsync(T entity)
        {
            await baseRepository.SaveOrEditAsync(entity).ConfigureAwait(false);
            return entity;
        }

        public virtual async Task<List<T>> GetAllAsync()
        {
            return await baseRepository.GetAll().ToListAsync().ConfigureAwait(false);
        }
    }
}