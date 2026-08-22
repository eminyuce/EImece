using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class FaqService : BaseEntityService<Faq>, IFaqService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IFaqRepository FaqRepository { get; set; }

        public FaqService(IFaqRepository repository) : base(repository)
        {
            FaqRepository = repository;
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking)

        public async Task<List<FaqDto>> GetStorefrontFaqsAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.FaqListAsync(language);
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => FaqRepository.GetStorefrontFaqsAsync(language, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        public List<FaqDto> GetStorefrontFaqs(int language)
        {
            var cacheKey = CacheKeys.FaqList(language);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => FaqRepository.GetStorefrontFaqs(language),
                AppConfig.CacheLongSeconds);
        }

        public async Task<List<Models.DTOs.Storefront.FaqSummaryDto>> GetStorefrontFaqSummariesAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.FaqListAsync(language) + "-Summary";
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => FaqRepository.GetStorefrontFaqSummariesAsync(language, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        public List<Models.DTOs.Storefront.FaqSummaryDto> GetStorefrontFaqSummaries(int language)
        {
            var cacheKey = CacheKeys.FaqList(language) + "-Summary";
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => FaqRepository.GetStorefrontFaqSummaries(language),
                AppConfig.CacheLongSeconds);
        }

        private void InvalidateFaqCaches()
        {
            DataCachingProvider.ClearByPrefix(CacheKeys.FaqPrefix);
        }

        /// <summary>
        /// FAQ active-entity lists live under the faq: family so the invalidator above evicts them.
        /// </summary>
        protected override string ActiveListCachePrefix
        {
            get { return CacheKeys.FaqPrefix; }
        }

        protected override void InvalidateCachesAfterMutation()
        {
            InvalidateFaqCaches();
        }

        #endregion

        #region Mutation & Invalidation

        public override Faq SaveOrEditEntity(Faq entity)
        {
            var saved = base.SaveOrEditEntity(entity);
            InvalidateFaqCaches();
            return saved;
        }

        public override async Task<Faq> SaveOrEditEntityAsync(Faq entity)
        {
            var saved = await base.SaveOrEditEntityAsync(entity).ConfigureAwait(false);
            InvalidateFaqCaches();
            return saved;
        }

        public override bool DeleteEntity(Faq entity)
        {
            var result = base.DeleteEntity(entity);
            InvalidateFaqCaches();
            return result;
        }

        public override async Task<bool> DeleteEntityAsync(Faq entity)
        {
            var result = await base.DeleteEntityAsync(entity).ConfigureAwait(false);
            InvalidateFaqCaches();
            return result;
        }

        #endregion
    }
}