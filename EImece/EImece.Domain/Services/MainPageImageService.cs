using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.DTOs.Storefront;
using EImece.Domain.Models.FrontModels;
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

        public async Task<List<StorefrontBannerDto>> GetStorefrontMainPageBannersAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await MainPageImageRepository.GetStorefrontMainPageBannersAsync(language, cancellationToken).ConfigureAwait(false);
        }

        public List<StorefrontBannerDto> GetStorefrontMainPageBanners(int language)
        {
            return MainPageImageRepository.GetStorefrontMainPageBanners(language);
        }

        #endregion

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
        }

        public MainPageViewModel GetMainPageViewModel(int language)
        {
            var result = new MainPageViewModel();
            int limit = AppConfig.HomePageMainProductCountLimit;

            result.MainPageProducts = ProductService.GetStorefrontMainPageProducts(limit, language);
            result.LatestProducts = ProductService.GetStorefrontLatestProducts(limit, language);
            result.CampaignProducts = ProductService.GetStorefrontCampaignProducts(limit, language);

            result.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, language).FirstOrDefault(r => r.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
            result.LatestStories = StoryService.GetStorefrontFeaturedStories(AppConfig.HomePageFeatureStoryCountLimit, language, 0);
            result.MainPageImages = GetStorefrontMainPageBanners(language);
            result.MainPageProductCategories = ProductCategoryService.GetStorefrontMainPageCategories(language);

            return result;
        }

        public async Task<MainPageViewModel> GetMainPageViewModelAsync(int language, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new MainPageViewModel();
            int limit = AppConfig.HomePageMainProductCountLimit;

            result.MainPageProducts = await ProductService.GetStorefrontMainPageProductsAsync(limit, language, cancellationToken).ConfigureAwait(false);
            result.LatestProducts = await ProductService.GetStorefrontLatestProductsAsync(limit, language, cancellationToken).ConfigureAwait(false);
            result.CampaignProducts = await ProductService.GetStorefrontCampaignProductsAsync(limit, language, cancellationToken).ConfigureAwait(false);

            var menus = await MenuService.GetActiveBaseContentsFromCacheAsync(true, language).ConfigureAwait(false);
            result.MainPageMenu = menus.FirstOrDefault(r => r.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));

            result.LatestStories = await StoryService.GetStorefrontFeaturedStoriesAsync(AppConfig.HomePageFeatureStoryCountLimit, language, 0, cancellationToken).ConfigureAwait(false);
            result.MainPageImages = await GetStorefrontMainPageBannersAsync(language, cancellationToken).ConfigureAwait(false);
            result.MainPageProductCategories = await ProductCategoryService.GetStorefrontMainPageCategoriesAsync(language, cancellationToken).ConfigureAwait(false);

            return result;
        }

        public FooterViewModel GetFooterViewModel(int language)
        {
            var result = new FooterViewModel();
            result.Menus = MenuService.GetActiveBaseContentsFromCache(true, language).ToList();
            result.ProductCategories = ProductCategoryService.GetStorefrontMainPageCategories(language);
            result.FooterLogo = SettingService.GetSettingObjectByKey(Constants.WebSiteLogo);
            result.CompanyName = SettingService.GetSettingObjectByKey(Constants.CompanyName);
            result.CompanyAddress = SettingService.GetSettingObjectByKey(Constants.CompanyAddress);
            result.FooterDescription = SettingService.GetSettingObjectByKey(Constants.FooterDescription, language);
            result.FooterEmailListDescription = SettingService.GetSettingObjectByKey(Constants.FooterEmailListDescription, language);
            result.FooterHtmlDescription = SettingService.GetSettingObjectByKey(Constants.FooterHtmlDescription, language);

            return result;
        }
    }
}