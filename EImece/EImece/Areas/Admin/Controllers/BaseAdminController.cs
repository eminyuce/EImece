using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Factories;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using EImece.Models;
using Ninject;
using SharkDev.Web.Controls.TreeView.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EImece.Areas.Admin.Controllers
{
    [AuthorizeRoles(Settings.AdministratorRole, Settings.EditorRole)]
    public abstract class BaseAdminController : Controller
    {
        protected const string TempDataReturnUrlReferrer = "TempDataReturnUrlReferrer"; 
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
        public ISubscriberService SubscriberService { get; set; }
        [Inject]
        public IFileStorageService FileStorageService { get; set; }
        [Inject]
        public ITemplateService TemplateService { get; set; }
        [Inject]
        public IListService ListService { get; set; }
        [Inject]
        public IListItemService ListItemService { get; set; }

        [Inject]
        public IEmailSender EmailSender { get; set; }
        [Inject]
        public ICacheProvider MemoryCacheProvider { get; set; }

        [Inject]
        public IMailTemplateService MailTemplateService { get; set; }


        private FilesHelper _filesHelper { get; set; }
        [Inject]
        public FilesHelper FilesHelper
        {
            get
            {
                _filesHelper.Init(Settings.DeleteURL, Settings.DeleteType, Settings.StorageRoot, Settings.UrlBase, Settings.TempPath, Settings.ServerMapPath);
                return _filesHelper;
            }
            set
            {
                _filesHelper = value;
            }
        }
        [Inject]
        public ApplicationDbContext ApplicationDbContext { get; set; }

        protected int SelectedLanguage
        {
            get
            {
                if (Session["SelectedLanguage"] != null)
                    return Session["SelectedLanguage"].ToInt(1);
                else
                {
                    return 1;
                };
            }
            set
            {
                Session["SelectedLanguage"] = value;
            }
        }
        protected static string AdminCultureCookieName = "_adminCulture";
        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            string cultureName = "tr-TR";

            HttpCookie cultureCookie = Request.Cookies[AdminCultureCookieName];
            if (cultureCookie != null)
            {
                cultureName = cultureCookie.Value;
            }


            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            if (Response.Cookies[AdminCultureCookieName] != null)
            {
                Response.Cookies[AdminCultureCookieName].Value = cultureName;
            }
            else
            {
                Response.Cookies.Add(cultureCookie);
            }
            
            return base.BeginExecuteCore(callback, state);
        }

        protected EImeceLanguage GetCurrentLanguage
        {
            get
            {
                return (EImeceLanguage)CurrentLanguage;
            }
        }
        protected int CurrentLanguage
        {
            get
            {
                string cultureName = null;
                HttpCookie cultureCookie = Request.Cookies[AdminCultureCookieName];
                if (cultureCookie != null)
                {
                    cultureName = cultureCookie.Value;
                    var selectedLang = EnumHelper.GetEnumFromDescription(cultureName, typeof(EImeceLanguage));
                    return selectedLang;

                }
                else
                {

                    return Settings.MainLanguage;
                }
            }
        }


        protected ActionResult RequestReturn(RedirectToRouteResult returnDefault)
        {
            var urlReferrer = Request.UrlReferrer;
            if (urlReferrer != null)
            {
                return Redirect(urlReferrer.ToStr());
            }
            else
            {
                return returnDefault;
            }
        }

        protected ActionResult ReturnTempUrl(String name)
        {
            if (!String.IsNullOrEmpty(TempData[TempDataReturnUrlReferrer].ToStr()))
            {
                MemoryCacheProvider.ClearAll();
                return Redirect(TempData[TempDataReturnUrlReferrer].ToStr());
            }
            else
            {
                return RedirectToAction(name);
            }
        }

        public BaseAdminController()
        {

        }
    }
}