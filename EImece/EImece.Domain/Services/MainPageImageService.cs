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
            var result = new MainPageViewModel();

            var activeProducts = ProductService.GetActiveProducts(language);
            result.MainPageProducts = activeProducts.Where(r => r.IsActive && r.MainPage && r.MainImageId > 0).OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).Take(AppConfig.HomePageMainProductCountLimit).ToList();
            result.LatestProducts = activeProducts.Where(r => r.IsActive && r.MainImageId > 0).OrderByDescending(r => r.UpdatedDate).Take(AppConfig.HomePageMainProductCountLimit).ToList();
            result.CampaignProducts = activeProducts.Where(r => r.IsActive && r.IsCampaign && r.MainImageId > 0).OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).Take(AppConfig.HomePageMainProductCountLimit).ToList();

            result.MainPageMenu = MenuService.GetActiveBaseContentsFromCache(true, language).FirstOrDefault(r => r.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
            // result.StoryIndexViewModel = StoryService.GetMainPageStories(1, language);
            result.LatestStories = StoryService.GetFeaturedStories(10, language,0).OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).Take(AppConfig.HomePageFeatureStoryCountLimit).ToList();
            result.MainPageImages = GetActiveBaseContentsFromCache(true, language);
            result.MainPageProductCategories = ProductCategoryService.GetMainPageProductCategories(language);

            return result;
        }


        public MainPageViewModel GetMainPageViewModelDto(int language)
        {
            var result = GetMainPageViewModel(language);
            result.MainPageImagesDto = Mapper.Map<System.Collections.Generic.List<EImece.Domain.Models.DTOs.MainPageImageDto>>(result.MainPageImages);
            result.MainPageProductCategoriesDto = Mapper.Map<System.Collections.Generic.List<EImece.Domain.Models.DTOs.ProductCategoryDto>>(result.MainPageProductCategories);
            result.MainPageProductsDto = Mapper.Map<System.Collections.Generic.List<EImece.Domain.Models.DTOs.ProductDto>>(result.MainPageProducts);
            result.LatestProductsDto = Mapper.Map<System.Collections.Generic.List<EImece.Domain.Models.DTOs.ProductDto>>(result.LatestProducts);
            result.CampaignProductsDto = Mapper.Map<System.Collections.Generic.List<EImece.Domain.Models.DTOs.ProductDto>>(result.CampaignProducts);
            result.MainPageMenuDto = Mapper.Map<EImece.Domain.Models.DTOs.MenuDto>(result.MainPageMenu);
            result.LatestStoriesDto = Mapper.Map<System.Collections.Generic.List<EImece.Domain.Models.DTOs.StoryDto>>(result.LatestStories);
            return result;
        }

        public FooterViewModel GetFooterViewModelDto(int language)
        {
            var result = GetFooterViewModel(language);
            result.MenusDto = Mapper.Map<System.Collections.Generic.List<EImece.Domain.Models.DTOs.MenuDto>>(result.Menus);
            result.ProductCategoriesDto = Mapper.Map<System.Collections.Generic.List<EImece.Domain.Models.DTOs.ProductCategoryDto>>(result.ProductCategories);
            result.FooterLogoDto = Mapper.Map<EImece.Domain.Models.DTOs.SettingDto>(result.FooterLogo);
            result.CompanyNameDto = Mapper.Map<EImece.Domain.Models.DTOs.SettingDto>(result.CompanyName);
            result.CompanyAddressDto = Mapper.Map<EImece.Domain.Models.DTOs.SettingDto>(result.CompanyAddress);
            result.FooterDescriptionDto = Mapper.Map<EImece.Domain.Models.DTOs.SettingDto>(result.FooterDescription);
            result.FooterHtmlDescriptionDto = Mapper.Map<EImece.Domain.Models.DTOs.SettingDto>(result.FooterHtmlDescription);
            result.FooterEmailListDescriptionDto = Mapper.Map<EImece.Domain.Models.DTOs.SettingDto>(result.FooterEmailListDescription);
            return result;
        }

        public FooterViewModel GetFooterViewModel(int language)
        {
            var result = new FooterViewModel();
            result.Menus = MenuService.GetActiveBaseContentsFromCache(true, language).ToList();
            result.ProductCategories = ProductCategoryService.GetMainPageProductCategories(language).ToList();
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