using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Observability.Telemetry;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class MainPageImageService : BaseContentService<MainPageImage>, IMainPageImageService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public IStoryService StoryService { get; set; }

        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; }

        private IMainPageImageRepository MainPageImageRepository { get; set; }

        public MainPageImageService(IMainPageImageRepository repository) : base(repository)
        {
            MainPageImageRepository = repository;
        }

        #region Storefront Read Methods (LINQ Projection, AsNoTracking, Main Entity Activation)

        [Timed("service.main_page_image.get_banners", "Time taken to get storefront main page banners")]
        public virtual async Task<List<StorefrontBannerDto>> GetStorefrontMainPageBannersAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cacheKey = CacheKeys.MainPageBannersAsync(language);
            return await DataCachingProvider.GetOrAddAsync(
                cacheKey,
                () => MainPageImageRepository.GetStorefrontMainPageBannersAsync(language, cancellationToken),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        [Timed("service.main_page_image.get_banners_sync")]
        public virtual List<StorefrontBannerDto> GetStorefrontMainPageBanners(int language)
        {
            var cacheKey = CacheKeys.MainPageBanners(language);
            return DataCachingProvider.GetOrAdd(
                cacheKey,
                () => MainPageImageRepository.GetStorefrontMainPageBanners(language),
                AppConfig.CacheLongSeconds);
        }

        private void InvalidateBannerCaches()
        {
            DataCachingProvider.ClearByPrefix(CacheKeys.BannerPrefix);
        }

        /// <summary>
        /// Banner active-content lists live under the banner: family so the invalidator above evicts them.
        /// </summary>
        protected override string ActiveListCachePrefix
        {
            get { return CacheKeys.BannerPrefix; }
        }

        protected override void InvalidateCachesAfterMutation()
        {
            InvalidateBannerCaches();
        }

        #endregion

        #region Mutation & Invalidation

        public override MainPageImage SaveOrEditEntity(MainPageImage entity)
        {
            var saved = base.SaveOrEditEntity(entity);
            InvalidateBannerCaches();
            return saved;
        }

        public override async Task<MainPageImage> SaveOrEditEntityAsync(MainPageImage entity)
        {
            var saved = await base.SaveOrEditEntityAsync(entity).ConfigureAwait(false);
            InvalidateBannerCaches();
            return saved;
        }

        public void DeleteMainPageImage(int id)
        {
            var item = MainPageImageRepository.GetSingle(id);
            if (item == null)
            {
                return;
            }

            if (item.MainImageId.HasValue)
            {
                FileStorageService.DeleteFileStorage(item.MainImageId.Value);
            }
            DeleteEntity(item);
            InvalidateBannerCaches();
        }

        public async Task DeleteMainPageImageAsync(int id)
        {
            var item = await MainPageImageRepository.GetSingleAsync(id).ConfigureAwait(false);
            if (item == null)
            {
                return;
            }

            if (item.MainImageId.HasValue)
            {
                await FileStorageService.DeleteFileStorageAsync(item.MainImageId.Value).ConfigureAwait(false);
            }
            await DeleteEntityAsync(item).ConfigureAwait(false);
            InvalidateBannerCaches();
        }

        #endregion

        #region ViewModels

        [Timed("service.main_page_image.get_view_model_sync")]
        public virtual MainPageViewModel GetMainPageViewModel(int language)
        {
            var result = new MainPageViewModel();
            int limit = AppConfig.HomePageMainProductCountLimit;

            result.MainPageProducts = ProductService.GetStorefrontMainPageProducts(limit, language);
            result.LatestProducts = ProductService.GetStorefrontLatestProducts(limit, language);
            result.CampaignProducts = ProductService.GetStorefrontCampaignProducts(limit, language);

            var pageDto = MenuService.GetStorefrontPageByMenuLink("home-index", language);
            if (pageDto != null)
            {
                result.MainPageMenu = new StorefrontMenuDto
                {
                    Id = pageDto.Id,
                    Name = pageDto.Name,
                    MenuLink = pageDto.MenuLink,
                    Description = pageDto.Description,
                    ShortDescription = pageDto.ShortDescription,
                    MainImageId = pageDto.MainImageId,
                    Position = pageDto.Position,
                    Lang = pageDto.Lang,
                    IsActive = pageDto.IsActive
                };
            }

            result.LatestStories = StoryService.GetStorefrontFeaturedStories(AppConfig.HomePageFeatureStoryCountLimit, language, 0);
            result.MainPageImages = GetStorefrontMainPageBanners(language);
            result.MainPageProductCategories = ProductCategoryService.GetStorefrontMainPageCategories(language);

            return result;
        }

        [Timed("service.main_page_image.get_view_model", "Time taken to get main page view model")]
        public virtual async Task<MainPageViewModel> GetMainPageViewModelAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new MainPageViewModel();
            int limit = AppConfig.HomePageMainProductCountLimit;

            result.MainPageProducts = await ProductService.GetStorefrontMainPageProductsAsync(limit, language, cancellationToken).ConfigureAwait(false);
            result.LatestProducts = await ProductService.GetStorefrontLatestProductsAsync(limit, language, cancellationToken).ConfigureAwait(false);
            result.CampaignProducts = await ProductService.GetStorefrontCampaignProductsAsync(limit, language, cancellationToken).ConfigureAwait(false);

            var pageDto = await MenuService.GetStorefrontPageByMenuLinkAsync("home-index", language, cancellationToken).ConfigureAwait(false);
            if (pageDto != null)
            {
                result.MainPageMenu = new StorefrontMenuDto
                {
                    Id = pageDto.Id,
                    Name = pageDto.Name,
                    MenuLink = pageDto.MenuLink,
                    Description = pageDto.Description,
                    ShortDescription = pageDto.ShortDescription,
                    MainImageId = pageDto.MainImageId,
                    Position = pageDto.Position,
                    Lang = pageDto.Lang,
                    IsActive = pageDto.IsActive
                };
            }

            result.LatestStories = await StoryService.GetStorefrontFeaturedStoriesAsync(AppConfig.HomePageFeatureStoryCountLimit, language, 0, cancellationToken).ConfigureAwait(false);
            result.MainPageImages = await GetStorefrontMainPageBannersAsync(language, cancellationToken).ConfigureAwait(false);
            result.MainPageProductCategories = await ProductCategoryService.GetStorefrontMainPageCategoriesAsync(language, cancellationToken).ConfigureAwait(false);

            return result;
        }

        [Timed("service.main_page_image.get_footer_view_model", "Time taken to get footer view model")]
        public virtual FooterViewModel GetFooterViewModel(int language)
        {
            var result = new FooterViewModel();
            result.Menus = MenuService.GetStorefrontMenuNavigation(language);
            result.ProductCategories = ProductCategoryService.GetStorefrontMainPageCategories(language);
            result.CompanyName = SettingService.GetCachedSettingValueDtoByKey(Constants.CompanyName);
            result.CompanyAddress = SettingService.GetCachedSettingValueDtoByKey(Constants.CompanyAddress);
            result.FooterDescription = SettingService.GetCachedSettingValueDtoByKey(Constants.FooterDescription, language);
            result.FooterEmailListDescription = SettingService.GetCachedSettingValueDtoByKey(Constants.FooterEmailListDescription, language);
            result.FooterHtmlDescription = SettingService.GetCachedSettingValueDtoByKey(Constants.FooterHtmlDescription, language);

            return result;
        }

        #endregion
    }
}