using EImece.Domain.Entities;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Linq;

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

        public void DeleteMainPageImage(int id)
        {
            var item = MainPageImageRepository.GetSingle(id);
            if (item.MainImageId.HasValue)
            {
                FileStorageService.DeleteFileStorage(item.MainImageId.Value);
            }
            DeleteEntity(item);
        }

        public MainPageViewModel GetMainPageViewModel(int language)
        {
            var cacheKey = String.Format("GetMainPageViewModel-{0}", language);
            MainPageViewModel result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = new MainPageViewModel();
                var queryables = ProductService.GetActiveProducts(language);
                result.MainPageProducts = queryables.Where(r => r.IsActive &&  r.MainPage && r.MainImageId > 0).OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).Take(8).ToList();
                result.LatestProducts = queryables.Where(r => r.IsActive && r.MainImageId > 0).OrderByDescending(r => r.UpdatedDate).Take(8).ToList();
                result.CampaignProducts = queryables.Where(r => r.IsActive && r.IsCampaign && r.MainImageId > 0).OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).Take(8).ToList();

                result.MainPageMenu = MenuService.GetActiveBaseContents(true, language).FirstOrDefault(r => r.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
                result.StoryIndexViewModel = StoryService.GetMainPageStories(1, language);
                result.LatestStories = StoryService.GetLatestStories(language, 4);
                result.MainPageImages = GetActiveBaseContents(true, language);
                result.MainPageProductCategories = ProductCategoryService.GetMainPageProductCategories(language);
                MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheMediumSeconds);
            }
            else
            {
                Logger.Info("GetMainPageViewModel Page is coming from cache:" + cacheKey+ " CacheDuration:" + MemoryCacheProvider.CacheDuration);
            }
            return result;
        }

        public FooterViewModel GetFooterViewModel(int language)
        {
            var cacheKey = String.Format("GetFooterViewModel-{0}", language);
            FooterViewModel result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = new FooterViewModel();
                result.Menus = MenuService.GetActiveBaseContents(true, language).ToList();
                result.ProductCategories = ProductCategoryService.GetMainPageProductCategories(language).ToList();
                result.FooterLogo = SettingService.GetSettingObjectByKey(Constants.WebSiteLogo);
                result.CompanyName = SettingService.GetSettingObjectByKey(Constants.CompanyName);
                result.CompanyAddress = SettingService.GetSettingObjectByKey(Constants.CompanyAddress);
                result.FooterDescription = SettingService.GetSettingObjectByKey(Constants.FooterDescription, language);
                result.FooterEmailListDescription = SettingService.GetSettingObjectByKey(Constants.FooterEmailListDescription, language);
                result.FooterHtmlDescription = SettingService.GetSettingObjectByKey(Constants.FooterHtmlDescription, language);
                MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheLongSeconds);
            }
            return result;
        }
    }
}