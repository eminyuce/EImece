using EImece.Domain.Entities;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Repositories.IRepositories;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public abstract class BaseEntityService<T> : BaseService<T> where T : BaseEntity
    {
        private const string STATE = "state";
        private const string MAIN_PAGE = "mainpage";
        private const string IMAGE_STATE = "imagestate";
        private const string IS_CAMPAIGN = "IsCampaign";
        private static readonly Logger BaseEntityServiceLogger = LogManager.GetCurrentClassLogger();

        private IBaseEntityRepository<T> baseEntityRepository { get; set; }

        protected BaseEntityService(IBaseEntityRepository<T> baseEntityRepository) : base(baseEntityRepository)
        {
            this.baseEntityRepository = baseEntityRepository;
        }

        protected BaseEntityService(IBaseEntityRepository<T> baseEntityRepository, bool IsCachingActivated) : base(baseEntityRepository)
        {
            this.IsCachingActivated = IsCachingActivated;
        }

        public virtual List<T> GetActiveBaseEntities(bool? isActive, int? language)
        {
            return baseEntityRepository.GetActiveBaseEntities(isActive, language);
        }

        public virtual List<T> GetActiveBaseEntitiesFromCache(bool? isActive, int? language)
        {
            // Hierarchical key under the service's CacheKeys family so ClearByPrefix
            // invalidation (product save, category save, ...) actually evicts it.
            String cacheKey = ActiveListCachePrefix + "activeentities:" + isActive + ":lang" + language;

            // Single-flight population: concurrent misses share one repository call.
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => baseEntityRepository.GetActiveBaseEntities(isActive, language),
                AppConfig.CacheLongSeconds);
        }

        public virtual async Task<List<T>> GetActiveBaseEntitiesAsync(bool? isActive, int? language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await baseEntityRepository.GetActiveBaseEntitiesAsync(isActive, language, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<List<T>> GetActiveBaseEntitiesFromCacheAsync(bool? isActive, int? language)
        {
            String cacheKey = ActiveListCachePrefix + "activeentities:" + isActive + ":lang" + language + AsyncCacheKeySuffix;

            // Single-flight population: concurrent misses share one repository call. The caller's
            // token is intentionally not forwarded here - one request cancelling would otherwise
            // fault the shared task that every other waiter is already awaiting.
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => baseEntityRepository.GetActiveBaseEntitiesAsync(isActive, language, CancellationToken.None),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        public virtual List<T> SearchEntities(Expression<Func<T, bool>> whereLambda, String search, int? language)
        {
            return baseEntityRepository.SearchEntities(whereLambda, search, language);
        }

        public virtual async Task<List<T>> SearchEntitiesAsync(Expression<Func<T, bool>> whereLambda, String search, int? language)
        {
            return await baseEntityRepository.SearchEntitiesAsync(whereLambda, search, language).ConfigureAwait(false);
        }

        public virtual new T SaveOrEditEntity(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentException("entity cannot be null");
            }
            if (entity.Id > 0)
            {
                entity.UpdatedDate = DateTime.Now;
            }
            else
            {
                entity.UpdatedDate = DateTime.Now;
                entity.CreatedDate = DateTime.Now;
            }
            var tmp = baseEntityRepository.SaveOrEdit(entity);
            return entity;
        }

        public virtual new async Task<T> SaveOrEditEntityAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentException("entity cannot be null");
            }
            if (entity.Id > 0)
            {
                entity.UpdatedDate = DateTime.Now;
            }
            else
            {
                entity.UpdatedDate = DateTime.Now;
                entity.CreatedDate = DateTime.Now;
            }
            await baseEntityRepository.SaveOrEditAsync(entity).ConfigureAwait(false);
            return entity;
        }

        public virtual void ChangeGridBaseEntityOrderingOrState(List<OrderingItem> values, String checkbox = "")
        {
            if (values == null)
            {
                throw new ArgumentException("values cannot be null");
            }
            bool isEdit = false;
            foreach (OrderingItem item in values)
            {
                var t = baseEntityRepository.GetSingle(item.Id);
                var baseContent = t as BaseEntity;
                if (baseContent != null)
                {
                    try
                    {
                        ApplyCheckboxOrPosition(baseContent, item, checkbox);
                        baseEntityRepository.Edit(t);
                        isEdit = true;
                    }
                    catch (Exception exception)
                    {
                        BaseEntityServiceLogger.Error(exception, "ChangeGridOrderingOrState<T> :" + item.Id, checkbox);
                    }
                }
            }
            if (isEdit)
            {
                baseEntityRepository.Save();
                // Grid state/position/main-page/campaign toggles bypass SaveOrEditEntity, so the
                // per-service invalidation that runs on a normal save never fired here. Storefront
                // caches (product lists, category trees, menus, ...) stayed stale until their TTL
                // expired. Each service now drops its own cache family via the override below.
                InvalidateCachesAfterMutation();
            }
        }

        public virtual async Task ChangeGridBaseEntityOrderingOrStateAsync(List<OrderingItem> values, String checkbox = "")
        {
            if (values == null)
            {
                throw new ArgumentException("values cannot be null");
            }
            bool isEdit = false;
            foreach (OrderingItem item in values)
            {
                var t = await baseEntityRepository.GetSingleAsync(item.Id).ConfigureAwait(false);
                var baseContent = t as BaseEntity;
                if (baseContent != null)
                {
                    try
                    {
                        ApplyCheckboxOrPosition(baseContent, item, checkbox);
                        baseEntityRepository.Edit(t);
                        isEdit = true;
                    }
                    catch (Exception exception)
                    {
                        BaseEntityServiceLogger.Error(exception, "ChangeGridOrderingOrState<T> :" + item.Id, checkbox);
                    }
                }
            }
            if (isEdit)
            {
                await baseEntityRepository.SaveAsync().ConfigureAwait(false);
                InvalidateCachesAfterMutation();
            }
        }

        /// <summary>
        /// Cache invalidation executed after a successful grid mutation (state / position /
        /// main-page / campaign toggles). Override in storefront services to drop the cache
        /// families affected by this entity; the base implementation does nothing.
        /// </summary>
        protected virtual void InvalidateCachesAfterMutation()
        {
        }

        private static void ApplyCheckboxOrPosition(BaseEntity baseContent, OrderingItem item, string checkbox)
        {
            if (string.IsNullOrEmpty(checkbox))
            {
                baseContent.Position = item.Position;
                return;
            }

            if (checkbox.Equals(STATE, StringComparison.InvariantCultureIgnoreCase))
            {
                baseContent.IsActive = item.IsActive;
                return;
            }

            if (checkbox.Equals(MAIN_PAGE, StringComparison.InvariantCultureIgnoreCase))
            {
                ApplyMainPageFlag(baseContent, item.IsActive);
                return;
            }

            if (checkbox.Equals(IMAGE_STATE, StringComparison.InvariantCultureIgnoreCase) && baseContent is BaseContent imageContent)
            {
                imageContent.ImageState = item.IsActive;
                return;
            }

            if (checkbox.Equals(IS_CAMPAIGN, StringComparison.InvariantCultureIgnoreCase) && baseContent is Product product)
            {
                product.IsCampaign = item.IsActive;
            }
        }

        private static void ApplyMainPageFlag(BaseEntity entity, bool isActive)
        {
            if (entity is Product product)
            {
                product.MainPage = isActive;
            }
            else if (entity is Story story)
            {
                story.MainPage = isActive;
            }
            else if (entity is ProductCategory productCategory)
            {
                productCategory.MainPage = isActive;
            }
            else if (entity is Brand brand)
            {
                brand.MainPage = isActive;
            }
            else if (entity is Menu menu)
            {
                menu.MainPage = isActive;
            }
        }
    }
}