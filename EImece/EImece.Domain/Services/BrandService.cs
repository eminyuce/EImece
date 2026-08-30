using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using NLog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using EImece.Domain.Factories.IFactories;
using System;

namespace EImece.Domain.Services
{
    public class BrandService : BaseContentService<Brand>, IBrandService
    {
        private readonly IBrandRepository BrandRepository;

        public BrandService(
            IBrandRepository repository,
            IEimeceCacheProvider dataCachingProvider,
            ISettingService settingService,
            IFileStorageService fileStorageService,
            IHttpContextFactory httpContextFactory,
            FilesHelper filesHelper)
            : base(repository, dataCachingProvider, settingService, fileStorageService, httpContextFactory, filesHelper)
        {
            BrandRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        public async Task<List<StorefrontBrandDto>> GetStorefrontBrandsAsync(int lang, int categoryId = 0, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (categoryId > 0)
            {
                // Category-page brand filters: cached under the brand: family (dropped by
                // InvalidateBrandCaches) and keyed by category + language.
                var catCacheKey = CacheKeys.BrandPrefix + "list:cat" + categoryId + ":lang" + lang;
                return await DataCachingProvider.GetOrAddAsync(
                    catCacheKey,
                    () => BrandRepository.GetStorefrontBrandsAsync(lang, categoryId, CancellationToken.None),
                    AppConfig.CacheMediumSeconds).ConfigureAwait(false);
            }

            var cacheKey = CacheKeys.BrandListAsync(lang);
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => BrandRepository.GetStorefrontBrandsAsync(lang, 0, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        public List<StorefrontBrandDto> GetStorefrontBrands(int lang, int categoryId = 0)
        {
            if (categoryId > 0)
            {
                var catCacheKey = CacheKeys.BrandPrefix + "list:cat" + categoryId + ":lang" + lang;
                return DataCachingProvider.GetOrAdd(
                    catCacheKey,
                    () => BrandRepository.GetStorefrontBrands(lang, categoryId),
                    AppConfig.CacheMediumSeconds);
            }

            var cacheKey = CacheKeys.BrandList(lang);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => BrandRepository.GetStorefrontBrands(lang, 0),
                AppConfig.CacheLongSeconds);
        }

        public async Task<StorefrontBrandDto> GetStorefrontBrandByIdAsync(int brandId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.BrandDetailAsync(brandId);
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => BrandRepository.GetStorefrontBrandByIdAsync(brandId, cancellationToken),
                AppConfig.CacheMediumSeconds).ConfigureAwait(false);
        }

        public StorefrontBrandDto GetStorefrontBrandById(int brandId)
        {
            var cacheKey = CacheKeys.BrandDetail(brandId);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => BrandRepository.GetStorefrontBrandById(brandId),
                AppConfig.CacheMediumSeconds);
        }

        private void InvalidateBrandCaches()
        {
            DataCachingProvider.ClearByPrefix(CacheKeys.BrandPrefix);
            DataCachingProvider.ClearByPrefix(CacheKeys.ProductListPrefix);
        }

        /// <summary>
        /// Brand active-content lists live under the brand: family so the invalidator above evicts them.
        /// </summary>
        protected override string ActiveListCachePrefix
        {
            get { return CacheKeys.BrandPrefix; }
        }

        protected override void InvalidateCachesAfterMutation()
        {
            InvalidateBrandCaches();
        }

        #endregion

        #region Mutation & Invalidation

        public override Brand SaveOrEditEntity(Brand entity)
        {
            var saved = base.SaveOrEditEntity(entity);
            InvalidateBrandCaches();
            return saved;
        }

        public override async Task<Brand> SaveOrEditEntityAsync(Brand entity)
        {
            var saved = await base.SaveOrEditEntityAsync(entity).ConfigureAwait(false);
            InvalidateBrandCaches();
            return saved;
        }

        #endregion

        #region Admin Methods (Full Entities)

        public List<Brand> GetAdminPageList(string search, int lang)
        {
            return BrandRepository.GetAdminPageList(search, lang);
        }

        public async Task<List<Brand>> GetAdminPageListAsync(string search, int lang)
        {
            return await BrandRepository.GetAdminPageListAsync(search, lang).ConfigureAwait(false);
        }

        public bool DeleteBrandById(int brandId)
        {
            var brand = BrandRepository.GetSingle(brandId);
            if (brand == null)
            {
                return false;
            }

            if (brand.MainImageId.HasValue && brand.MainImageId.Value > 0)
            {
                FileStorageService.DeleteFileStorage(brand.MainImageId.Value);
            }

            var deleted = BrandRepository.DeleteByWhereCondition(r => r.Id == brandId);
            if (deleted)
            {
                InvalidateBrandCaches();
            }
            return deleted;
        }

        public async Task<bool> DeleteBrandByIdAsync(int brandId)
        {
            var brand = await BrandRepository.GetSingleAsync(brandId).ConfigureAwait(false);
            if (brand == null)
            {
                return false;
            }

            if (brand.MainImageId.HasValue && brand.MainImageId.Value > 0)
            {
                await FileStorageService.DeleteFileStorageAsync(brand.MainImageId.Value).ConfigureAwait(false);
            }

            var deleted = await BrandRepository.DeleteByWhereConditionAsync(r => r.Id == brandId).ConfigureAwait(false);
            if (deleted)
            {
                InvalidateBrandCaches();
            }
            return deleted;
        }

        public virtual new void DeleteBaseEntity(List<string> values)
        {
            if (values == null)
            {
                return;
            }

            foreach (var v in values)
            {
                DeleteBrandById(v.ToInt());
            }
        }

        public virtual new async Task DeleteBaseEntityAsync(List<string> values)
        {
            if (values == null)
            {
                return;
            }

            foreach (var v in values)
            {
                await DeleteBrandByIdAsync(v.ToInt()).ConfigureAwait(false);
            }
        }

        public Brand GetBrandById(int brandId)
        {
            return BrandRepository.GetSingle(brandId);
        }

        public List<Brand> GetBrandsIfAnyProductExists(int lang, int categoryId = 0)
        {
            return BrandRepository.GetBrandsIfAnyProductExists(lang, categoryId);
        }

        public async Task<List<Brand>> GetBrandsIfAnyProductExistsAsync(int lang, int categoryId = 0)
        {
            return await BrandRepository.GetBrandsIfAnyProductExistsAsync(lang, categoryId).ConfigureAwait(false);
        }

        #endregion
    }
}