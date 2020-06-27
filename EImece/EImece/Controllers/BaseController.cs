using EImece.Domain;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.CodeDom;
using System.Net;
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
                _filesHelper.InitFilesMediaFolder();
                _filesHelper.IsCachingActive = AppConfig.IsCacheActive;
                return _filesHelper;
            }
            set
            {
                _filesHelper = value;
            }
        }

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            setIsCachingActive(AppConfig.IsCacheActive);
            return base.BeginExecuteCore(callback, state);
        }

        public void CreateLanguageCookie(EImeceLanguage selectedLanguage, string cookieName)
        {
            String cultureName = EnumHelper.GetEnumDescription(selectedLanguage);
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            HttpCookie cultureCookie  = new HttpCookie(cookieName);
            cultureCookie.Values[Constants.ELanguage] = ((int)selectedLanguage)+"";
            cultureCookie.Values["LastVisit"] = DateTime.Now.ToString();
            cultureCookie.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Add(cultureCookie);
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
                HttpCookie cultureCookie = Request.Cookies[Constants.CultureCookieName];
                if (cultureCookie != null)
                {
                    return cultureCookie.Values[Constants.ELanguage].ToInt();

                }
                else
                {
                    return AppConfig.MainLanguage;
                }
            }
        }
    }
}