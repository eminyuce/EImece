using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class TagService : BaseEntityService<Tag>, ITagService
    {
        private static readonly Logger TagServiceLogger = LogManager.GetCurrentClassLogger();

        private ITagRepository TagRepository { get; set; }

        public TagService(ITagRepository repository) : base(repository)
        {
            TagRepository = repository;
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        public async Task<List<StorefrontTagDto>> GetStorefrontProductTagsAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.ProductTagsAsync(language);
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => TagRepository.GetStorefrontProductTagsAsync(language, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        public List<StorefrontTagDto> GetStorefrontProductTags(int language)
        {
            var cacheKey = CacheKeys.ProductTags(language);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => TagRepository.GetStorefrontProductTags(language),
                AppConfig.CacheLongSeconds);
        }

        public async Task<List<StorefrontTagDto>> GetStorefrontTagsWithStoryCountsAsync(int language, int minStoryCount = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.StoryTagsAsync(language) + $":min{minStoryCount}";
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => TagRepository.GetStorefrontTagsWithStoryCountsAsync(language, minStoryCount, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        public List<StorefrontTagDto> GetStorefrontTagsWithStoryCounts(int language, int minStoryCount = 1)
        {
            var cacheKey = CacheKeys.StoryTags(language) + $":min{minStoryCount}";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => TagRepository.GetStorefrontTagsWithStoryCounts(language, minStoryCount),
                AppConfig.CacheLongSeconds);
        }

        public async Task<List<StorefrontTagDto>> GetStorefrontTagsWithEntityCountsAsync(int language, int minEntityCount = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.TagPrefix + $"entity_counts:lang{language}:min{minEntityCount}:async";
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => TagRepository.GetStorefrontTagsWithEntityCountsAsync(language, minEntityCount, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        public List<StorefrontTagDto> GetStorefrontTagsWithEntityCounts(int language, int minEntityCount = 1)
        {
            var cacheKey = CacheKeys.TagPrefix + $"entity_counts:lang{language}:min{minEntityCount}";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => TagRepository.GetStorefrontTagsWithEntityCounts(language, minEntityCount),
                AppConfig.CacheLongSeconds);
        }

        public async Task<StorefrontTagDto> GetStorefrontTagByIdAsync(int tagId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await TagRepository.GetStorefrontTagByIdAsync(tagId, cancellationToken).ConfigureAwait(false);
        }

        public StorefrontTagDto GetStorefrontTagById(int tagId)
        {
            return TagRepository.GetStorefrontTagById(tagId);
        }

        private void InvalidateTagCaches()
        {
            DataCachingProvider.ClearByPrefix(CacheKeys.TagPrefix);
            DataCachingProvider.ClearByPrefix(CacheKeys.ProductListPrefix);
            DataCachingProvider.ClearByPrefix(CacheKeys.StoryPrefix);
        }

        #endregion

        #region Mutation & Invalidation

        public override Tag SaveOrEditEntity(Tag entity)
        {
            var saved = base.SaveOrEditEntity(entity);
            InvalidateTagCaches();
            return saved;
        }

        public override async Task<Tag> SaveOrEditEntityAsync(Tag entity)
        {
            var saved = await base.SaveOrEditEntityAsync(entity).ConfigureAwait(false);
            InvalidateTagCaches();
            return saved;
        }

        #endregion

        #region Admin Methods (Full Entities)

        public List<Tag> GetAdminPageList(String search, int language)
        {
            return TagRepository.GetAdminPageList(search, language);
        }

        public async Task<List<Tag>> GetAdminPageListAsync(String search, int language)
        {
            return await TagRepository.GetAdminPageListAsync(search, language).ConfigureAwait(false);
        }

        public void DeleteTagById(int tagId)
        {
            var tag = GetTagById(tagId);
            ProductTagRepository.DeleteByWhereCondition(r => r.TagId == tagId);
            StoryTagRepository.DeleteByWhereCondition(r => r.TagId == tagId);
            DeleteEntity(tag);
            InvalidateTagCaches();
        }

        public async Task DeleteTagByIdAsync(int tagId)
        {
            var tag = GetTagById(tagId);
            await ProductTagRepository.DeleteByWhereConditionAsync(r => r.TagId == tagId).ConfigureAwait(false);
            await StoryTagRepository.DeleteByWhereConditionAsync(r => r.TagId == tagId).ConfigureAwait(false);
            await DeleteEntityAsync(tag).ConfigureAwait(false);
            InvalidateTagCaches();
        }

        public Tag GetTagById(int tagId)
        {
            return TagRepository.GetTagById(tagId);
        }

        public virtual new void DeleteBaseEntity(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    DeleteTagById(id);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                TagServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                TagServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public virtual new async Task DeleteBaseEntityAsync(List<string> values)
        {
            try
            {
                foreach (String v in values)
                {
                    var id = v.ToInt();
                    await DeleteTagByIdAsync(id).ConfigureAwait(false);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var message = ExceptionHelper.GetDbEntityValidationExceptionDetail(ex);
                TagServiceLogger.Error(ex, "DbEntityValidationException:" + message);
            }
            catch (Exception exception)
            {
                TagServiceLogger.Error(exception, "DeleteBaseEntity :" + String.Join(",", values));
            }
        }

        public List<Tag> GetProductTags(int language)
        {
            String cacheKey = String.Format(this.GetType().FullName + "-GetProductTags-{0}", language);

            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => TagRepository.GetProductTags(language),
                AppConfig.CacheLongSeconds);
        }

        public async Task<List<Tag>> GetProductTagsAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            String cacheKey = String.Format(this.GetType().FullName + "-GetProductTags-{0}", language) + AsyncCacheKeySuffix;

            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => TagRepository.GetProductTagsAsync(language, CancellationToken.None),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        public List<Tag> GetTagsWithEntityCounts(int language, int minEntityCount = 1)
        {
            String cacheKey = String.Format(
                this.GetType().FullName + "-GetTagsWithEntityCounts-{0}-{1}",
                language,
                minEntityCount);

            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => TagRepository.GetTagsWithEntityCounts(language, minEntityCount),
                AppConfig.CacheLongSeconds);
        }

        public List<Tag> GetTagsWithStoryCounts(int language, int minStoryCount = 1)
        {
            String cacheKey = String.Format(
                this.GetType().FullName + "-GetTagsWithStoryCounts-v2-{0}-{1}",
                language,
                minStoryCount);

            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => TagRepository.GetTagsWithStoryCounts(language, minStoryCount),
                AppConfig.CacheLongSeconds);
        }

        public async Task<List<Tag>> GetTagsWithStoryCountsAsync(int language, int minStoryCount = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            String cacheKey = String.Format(
                this.GetType().FullName + "-GetTagsWithStoryCounts-v2-{0}-{1}",
                language,
                minStoryCount) + AsyncCacheKeySuffix;

            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => TagRepository.GetTagsWithStoryCountsAsync(language, minStoryCount, CancellationToken.None),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        #endregion
    }
}