using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class StoryCategoryService : BaseContentService<StoryCategory>, IStoryCategoryService
    {
        private static readonly Logger StoryCategoryServiceLogger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IStoryService StoryService { get; set; }

        private IStoryCategoryRepository StoryCategoryRepository { get; set; }

        public StoryCategoryService(IStoryCategoryRepository repository) : base(repository)
        {
            StoryCategoryRepository = repository;
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        public async Task<List<StorefrontCategoryDto>> GetStorefrontActiveStoryCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.StoryCategoriesAsync(language);
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => StoryCategoryRepository.GetStorefrontActiveStoryCategoriesAsync(language, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        public List<StorefrontCategoryDto> GetStorefrontActiveStoryCategories(int language)
        {
            var cacheKey = CacheKeys.StoryCategories(language);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => StoryCategoryRepository.GetStorefrontActiveStoryCategories(language),
                AppConfig.CacheLongSeconds);
        }

        public async Task<StorefrontCategoryDto> GetStorefrontStoryCategoryByIdAsync(int storyCategoryId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.StoryPrefix + $"cat:{storyCategoryId}:async";
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => StoryCategoryRepository.GetStorefrontStoryCategoryByIdAsync(storyCategoryId, cancellationToken),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        public StorefrontCategoryDto GetStorefrontStoryCategoryById(int storyCategoryId)
        {
            var cacheKey = CacheKeys.StoryPrefix + $"cat:{storyCategoryId}";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => StoryCategoryRepository.GetStorefrontStoryCategoryById(storyCategoryId),
                AppConfig.CacheMediumSeconds);
        }

        private void InvalidateStoryCategoryCaches()
        {
            DataCachingProvider.ClearByPrefix(CacheKeys.StoryPrefix);
            DataCachingProvider.ClearByPrefix(CacheKeys.MenuPrefix);
        }

        /// <summary>
        /// Story-category active lists live under the story: family so the invalidator above evicts them.
        /// </summary>
        protected override string ActiveListCachePrefix
        {
            get { return CacheKeys.StoryPrefix; }
        }

        protected override void InvalidateCachesAfterMutation()
        {
            InvalidateStoryCategoryCaches();
        }

        #endregion

        #region Mutation & Invalidation

        public override StoryCategory SaveOrEditEntity(StoryCategory entity)
        {
            var saved = base.SaveOrEditEntity(entity);
            InvalidateStoryCategoryCaches();
            return saved;
        }

        public override async Task<StoryCategory> SaveOrEditEntityAsync(StoryCategory entity)
        {
            var saved = await base.SaveOrEditEntityAsync(entity).ConfigureAwait(false);
            InvalidateStoryCategoryCaches();
            return saved;
        }

        #endregion

        #region Admin Methods (Full Entities)

        public StoryCategory GetStoryCategoryById(int storyCategoryId)
        {
            return StoryCategoryRepository.GetStoryCategoryById(storyCategoryId);
        }

        public void DeleteStoryCategoryById(int storyCategoryId)
        {
            var storyCategory = GetStoryCategoryById(storyCategoryId);
            if (storyCategory == null) return;

            if (storyCategory.MainImageId.HasValue)
            {
                FileStorageService.DeleteFileStorage(storyCategory.MainImageId.Value);
            }
            var storyIdList = storyCategory.Stories != null ? storyCategory.Stories.Select(r => r.Id).ToList() : new List<int>();
            foreach (var id in storyIdList)
            {
                StoryService.DeleteStoryById(id);
            }
            DeleteEntity(storyCategory);
            InvalidateStoryCategoryCaches();
        }

        public async Task DeleteStoryCategoryByIdAsync(int storyCategoryId)
        {
            var storyCategory = await StoryCategoryRepository.GetSingleAsync(storyCategoryId).ConfigureAwait(false);
            if (storyCategory == null) return;

            if (storyCategory.MainImageId.HasValue)
            {
                await FileStorageService.DeleteFileStorageAsync(storyCategory.MainImageId.Value).ConfigureAwait(false);
            }
            var storyIdList = storyCategory.Stories != null ? storyCategory.Stories.Select(r => r.Id).ToList() : new List<int>();
            foreach (var id in storyIdList)
            {
                await StoryService.DeleteStoryByIdAsync(id).ConfigureAwait(false);
            }
            await DeleteEntityAsync(storyCategory).ConfigureAwait(false);
            InvalidateStoryCategoryCaches();
        }

        public virtual new void DeleteBaseEntity(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    DeleteStoryCategoryById(id);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                StoryCategoryServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                StoryCategoryServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public virtual new async Task DeleteBaseEntityAsync(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    await DeleteStoryCategoryByIdAsync(id).ConfigureAwait(false);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                StoryCategoryServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                StoryCategoryServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public List<StoryCategory> GetActiveStoryCategories(int language)
        {
            return StoryCategoryRepository.GetActiveStoryCategories(language);
        }

        public async Task<List<StoryCategory>> GetActiveStoryCategoriesAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await StoryCategoryRepository.GetActiveStoryCategoriesAsync(language, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}