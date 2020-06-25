using EImece.Domain;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public abstract class BaseController : Controller
    {
        [Inject]
        public IEntityFactory EntityFactory { get; set; }

        [Inject]
        public IMainPageImageService MainPageImageService { get; set; }

        [Inject]
        public ISettingService SettingService { get; set; }

        [Inject]
        public IProductService ProductService { get; set; }

        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; }

        [Inject]
        public IMenuService MenuService { get; set; }

        [Inject]
        public IStoryService StoryService { get; set; }

        [Inject]
        public IStoryCategoryService StoryCategoryService { get; set; }

        [Inject]
        public ITagService TagService { get; set; }

        [Inject]
        public ITagCategoryService TagCategoryService { get; set; }

        [Inject]
        public ISubscriberService SubsciberService { get; set; }

        [Inject]
        public IFileStorageService FileStorageService { get; set; }

        [Inject]
        public ITemplateService TemplateService { get; set; }

        [Inject]
        public IMailTemplateService MailTemplateService { get; set; }

        [Inject]
        public IEmailSender EmailSender { get; set; }

        [Inject]
        public RazorEngineHelper RazorEngineHelper { get; set; }

        private FilesHelper _filesHelper { get; set; }

        [Inject]
        public FilesHelper FilesHelper
        {
            get
            {
                _filesHelper.Init(Constants.DeleteURL, Constants.DeleteType,AppConfig.StorageRoot, Constants.UrlBase, Constants.TempPath, Constants.ServerMapPath);
                _filesHelper.IsCachingActive = AppConfig.IsCacheActive;
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
            setIsCachingActive(AppConfig.IsCacheActive);
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

        private void setIsCachingActive(bool IsCachingActive)
        {
            ProductService.IsCachingActive = IsCachingActive;
            ProductCategoryService.IsCachingActive = IsCachingActive;
            MainPageImageService.IsCachingActive = IsCachingActive;
            SettingService.IsCachingActive = IsCachingActive;
            ProductService.IsCachingActive = IsCachingActive;
            ProductCategoryService.IsCachingActive = IsCachingActive;
            MenuService.IsCachingActive = IsCachingActive;
            StoryService.IsCachingActive = IsCachingActive;
            StoryCategoryService.IsCachingActive = IsCachingActive;
            TagService.IsCachingActive = IsCachingActive;
            TagCategoryService.IsCachingActive = IsCachingActive;
            FileStorageService.IsCachingActive = IsCachingActive;
            TemplateService.IsCachingActive = IsCachingActive;
            MailTemplateService.IsCachingActive = IsCachingActive;
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
                    return EnumHelper.GetEnumFromDescription(cultureName, typeof(EImeceLanguage));
                }
                else
                {
                    return AppConfig.MainLanguage;
                }
            }
        }
    }
}