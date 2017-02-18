using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Factories;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public abstract class BaseController : Controller
    {
        [Inject]
        public ICacheProvider MemoryCacheProvider { get; set; }

        [Inject]
        public IEntityFactory EntityFactory { get; set; }

        private IMainPageImageService _mainPageImageService { get; set; }
        [Inject]
        public IMainPageImageService MainPageImageService
        {
            get
            {
                _mainPageImageService.IsCachingActive = Settings.IsCacheActive;
                return _mainPageImageService;
            }
            set { _mainPageImageService = value; }
        }

        private ISettingService _settingService { get; set; }
        [Inject]
        public ISettingService SettingService
        {
            get
            {
                _settingService.IsCachingActive = Settings.IsCacheActive;
                return _settingService;
            }
            set { _settingService = value; }
        }


        private IProductService _productService { get; set; }
        [Inject]
        public IProductService ProductService
        {
            get
            {
                _productService.IsCachingActive = Settings.IsCacheActive;
                return _productService;
            }
            set { _productService = value; }
        }


        private IProductCategoryService _productCategoryService { get; set; }
        [Inject]
        public IProductCategoryService ProductCategoryService
        {
            get
            {
                _productCategoryService.IsCachingActive = Settings.IsCacheActive;
                return _productCategoryService;
            }
            set { _productCategoryService = value; }
        }
        private IMenuService _menuService { get; set; }
        [Inject]
        public IMenuService MenuService
        {
            get
            {
                _menuService.IsCachingActive = Settings.IsCacheActive;
                return _menuService;
            }
            set { _menuService = value; }
        }

        private IStoryService _storyService { get; set; }
        [Inject]
        public IStoryService StoryService
        {
            get
            {
                _storyService.IsCachingActive = Settings.IsCacheActive;
                return _storyService;
            }
            set { _storyService = value; }
        }
        private IStoryCategoryService _storyCategoryService { get; set; }
        [Inject]
        public IStoryCategoryService StoryCategoryService
        {
            get
            {
                _storyCategoryService.IsCachingActive = Settings.IsCacheActive;
                return _storyCategoryService;
            }
            set { _storyCategoryService = value; }
        }

        private ITagService _tagService { get; set; }
        [Inject]
        public ITagService TagService
        {
            get
            {
                _tagService.IsCachingActive = Settings.IsCacheActive;
                return _tagService;
            }
            set { _tagService = value; }
        }

        private ITagCategoryService _tagCategoryService { get; set; }
        [Inject]
        public ITagCategoryService TagCategoryService
        {
            get
            {
                _tagCategoryService.IsCachingActive = Settings.IsCacheActive;
                return _tagCategoryService;
            }
            set { _tagCategoryService = value; }
        }

        private ISubscriberService _subsciberService { get; set; }
        [Inject]
        public ISubscriberService SubsciberService
        {
            get
            {
                _subsciberService.IsCachingActive = Settings.IsCacheActive;
                return _subsciberService;
            }
            set { _subsciberService = value; }
        }
        private IFileStorageService _fileStorageService { get; set; }
        [Inject]
        public IFileStorageService FileStorageService
        {
            get
            {
                _fileStorageService.IsCachingActive = Settings.IsCacheActive;
                return _fileStorageService;
            }
            set { _fileStorageService = value; }
        }

        private ITemplateService _templateService { get; set; }
        [Inject]
        public ITemplateService TemplateService
        {
            get
            {
                _templateService.IsCachingActive = Settings.IsCacheActive;
                return _templateService;
            }
            set { _templateService = value; }
        }


        [Inject]
        public IEmailSender EmailSender { get; set; }
        private FilesHelper _filesHelper { get; set; }
        [Inject]
        public FilesHelper FilesHelper
        {
            get
            {
                _filesHelper.Init(Settings.DeleteURL, Settings.DeleteType, Settings.StorageRoot, Settings.UrlBase, Settings.TempPath, Settings.ServerMapPath);
                _filesHelper.IsCachingActive = Settings.IsCacheActive;
                return _filesHelper;
            }
            set
            {
                _filesHelper = value;
            }
        }
        protected static string CultureCookieName = "_culture";
        protected override IAsyncResult BeginExecuteCore(System.AsyncCallback callback, object state)
        {
            string cultureName = "tr-TR";

            HttpCookie cultureCookie = Request.Cookies[CultureCookieName];
            if (cultureCookie != null)
            {
                cultureName = cultureCookie.Value;
            }


         //   cultureName = CultureHelper.GetImplementedCulture(cultureName);

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            if (Response.Cookies[CultureCookieName] != null)
            {
                Response.Cookies[CultureCookieName].Value = cultureName;
            }
            else
            {
                Response.Cookies.Add(cultureCookie);
            }

            return base.BeginExecuteCore(callback, state);
        }
        protected int CurrentLanguage
        {
            get
            {
                string cultureName = null;
                HttpCookie cultureCookie = Request.Cookies[CultureCookieName];
                if (cultureCookie != null)
                {
                    cultureName = cultureCookie.Value;
                    var selectedLang = EnumHelper.GetEnumFromDescription(cultureName, EImeceLanguage.English.GetType());
                    return selectedLang;

                }
                else
                {

                    return Settings.MainLanguage;
                }
            }
        }

    }
}