using AutoMapper;
using EImece.Domain.Entities;
using EImece.Domain.Models.DTOs;
using EImece.Domain.Models.FrontModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Linq;
using System.Collections.Generic;

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
            result.MainPageProducts = activeProducts.Where(r => r.IsActive && r.MainPage && r.MainImageId > 0).OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).Take(AppConfig.HomePageMainProductCountLimit).Select(p => Mapper.Map<ProductDto>(p)).ToList();
            result.LatestProducts = activeProducts.Where(r => r.IsActive && r.MainImageId > 0).OrderByDescending(r => r.UpdatedDate).Take(AppConfig.HomePageMainProductCountLimit).Select(p => Mapper.Map<ProductDto>(p)).ToList();
            result.CampaignProducts = activeProducts.Where(r => r.IsActive && r.IsCampaign && r.MainImageId > 0).OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).Take(AppConfig.HomePageMainProductCountLimit).Select(p => Mapper.Map<ProductDto>(p)).ToList();

            result.MainPageMenu = MenuService.GetActiveBaseContentsFromCacheAsDtos(true, language).FirstOrDefault(r => r.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
            // result.StoryIndexViewModel = StoryService.GetMainPageStories(1, language);
            result.LatestStories = StoryService.GetFeaturedStories(10, language,0).OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).Take(AppConfig.HomePageFeatureStoryCountLimit).Select(s => Mapper.Map<StoryDto>(s)).ToList();
            result.MainPageImages = GetActiveBaseContentsFromCache(true, language).Select(mpi => Mapper.Map<MainPageImageDto>(mpi)).ToList();
            result.MainPageProductCategories = ProductCategoryService.GetMainPageProductCategories(language).Select(pc => Mapper.Map<ProductCategoryDto>(pc)).ToList();

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
        
        public MainPageViewModel GetMainPageDtoViewModel(int language)
        {
            var result = new MainPageViewModel();

            var activeProducts = ProductService.GetActiveProducts(language);
            result.MainPageProducts = activeProducts.Where(r => r.IsActive && r.MainPage && r.MainImageId > 0).OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).Take(AppConfig.HomePageMainProductCountLimit).Select(p => Mapper.Map<ProductDto>(p)).ToList();
            result.LatestProducts = activeProducts.Where(r => r.IsActive && r.MainImageId > 0).OrderByDescending(r => r.UpdatedDate).Take(AppConfig.HomePageMainProductCountLimit).Select(p => Mapper.Map<ProductDto>(p)).ToList();
            result.CampaignProducts = activeProducts.Where(r => r.IsActive && r.IsCampaign && r.MainImageId > 0).OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).Take(AppConfig.HomePageMainProductCountLimit).Select(p => Mapper.Map<ProductDto>(p)).ToList();

            result.MainPageMenu = MenuService.GetActiveBaseContentsFromCacheAsDtos(true, language).FirstOrDefault(r => r.MenuLink.Equals("home-index", StringComparison.InvariantCultureIgnoreCase));
            // result.StoryIndexViewModel = StoryService.GetMainPageStories(1, language);
            result.LatestStories = StoryService.GetFeaturedStories(10, language,0).OrderBy(r => r.Position).ThenByDescending(r => r.UpdatedDate).Take(AppConfig.HomePageFeatureStoryCountLimit).Select(s => Mapper.Map<StoryDto>(s)).ToList();
            result.MainPageImages = GetActiveBaseContentsFromCache(true, language).Select(mpi => Mapper.Map<MainPageImageDto>(mpi)).ToList();
            result.MainPageProductCategories = ProductCategoryService.GetMainPageProductCategories(language).Select(pc => Mapper.Map<ProductCategoryDto>(pc)).ToList();

            return result;
        }
    }
}