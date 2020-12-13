using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EImece.Areas.Admin.Controllers
{
    [AuthorizeRoles(Constants.AdministratorRole, Constants.EditorRole)]
    public abstract class BaseAdminController : Controller
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
        public IProductCommentService ProductCommentService { get; set; }

        [Inject]
        public IProductCategoryService ProductCategoryService { get; set; }

        [Inject]
        public IMenuService MenuService { get; set; }

        [Inject]
        public IStoryService StoryService { get; set; }

        [Inject]
        public IBrandService BrandService { get; set; }

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

        [Inject]
        public IOrderService OrderService { get; set; }

        [Inject]
        public IOrderProductService OrderProductService { get; set; }

        [Inject]
        public IFaqService FaqService { get; set; }

        private FilesHelper _filesHelper { get; set; }

        public BaseAdminController()
        {
            TempData[Constants.TempDataReturnUrlReferrer] = "";
        }

        protected override IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state)
        {
            var IsCachingActivated = false;
            FileStorageService.IsCachingActivated = IsCachingActivated; 
            ListItemService.IsCachingActivated = IsCachingActivated;  
            ListService.IsCachingActivated = IsCachingActivated; 
            MailTemplateService.IsCachingActivated = IsCachingActivated; 
            MainPageImageService.IsCachingActivated = IsCachingActivated;  
            MenuService.IsCachingActivated = IsCachingActivated; 
            ProductCategoryService.IsCachingActivated = IsCachingActivated; 
            ProductService.IsCachingActivated = IsCachingActivated;  
            SettingService.IsCachingActivated = IsCachingActivated;  
            StoryCategoryService.IsCachingActivated = IsCachingActivated; 
            StoryService.IsCachingActivated = IsCachingActivated; 
            SubscriberService.IsCachingActivated = IsCachingActivated; 
            TagCategoryService.IsCachingActivated = IsCachingActivated;  
            TagService.IsCachingActivated = IsCachingActivated;  
            TemplateService.IsCachingActivated = IsCachingActivated; 
            OrderService.IsCachingActivated = IsCachingActivated;  
            OrderProductService.IsCachingActivated = IsCachingActivated;  
            FaqService.IsCachingActivated = IsCachingActivated;  
            ProductCommentService.IsCachingActivated = IsCachingActivated; 
            BrandService.IsCachingActivated = IsCachingActivated;  
            return base.BeginExecute(requestContext, callback, state);
        }

        [Inject]
        public FilesHelper FilesHelper
        {
            get
            {
                _filesHelper.InitFilesMediaFolder();
                return _filesHelper;
            }
            set
            {
                _filesHelper = value;
            }
        }

        [Inject]
        public RazorEngineHelper RazorEngineHelper { get; set; }

        protected int SelectedLanguage
        {
            get
            {
                if (Session[Constants.SelectedLanguage] != null)
                {
                    return Session[Constants.SelectedLanguage].ToInt(1);
                }
                else
                {
                    return AppConfig.MainLanguage;
                }
            }
            set
            {
                Session[Constants.SelectedLanguage] = value;
            }
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
                var languagesText = AppConfig.ApplicationLanguages;
                var languages = Regex.Split(languagesText, @",").Select(r => r.Trim()).Where(s => !String.IsNullOrEmpty(s)).ToList();
                if (languages.Count > 1)
                {
                    HttpCookie cultureCookie = Request.Cookies[Constants.AdminCultureCookieName];
                    if (cultureCookie != null)
                    {
                        return cultureCookie.Values[Constants.ELanguage].ToInt();
                    }
                    else
                    {
                        return AppConfig.MainLanguage;
                    }
                }
                else
                {
                    return AppConfig.MainLanguage;
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
        
        protected ActionResult DownloadFile<T>(IEnumerable<T> result, string fileName)
        {
            DataTable dt = GeneralHelper.LINQToDataTable(result);
            dt.TableName = fileName;
            return DownloadFileDataTable(dt, fileName);
        }

        protected ActionResult ReturnIndexIfNotUrlReferrer(String action)
        {
            if (Request.UrlReferrer == null || Request.UrlReferrer.ToStr().ToLower().Contains("saveoredit"))
            {
                return RedirectToAction(action);
            }
            else
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
        }

        protected ActionResult ReturnIndexIfNotUrlReferrer(String action, object routeValues)
        {
            if (Request.UrlReferrer == null || Request.UrlReferrer.ToStr().ToLower().Contains("saveoredit"))
            {
                return RedirectToAction(action, routeValues);
            }
            else
            {
                return Redirect(Request.UrlReferrer.ToString());
            }
        }

        protected ActionResult DownloadFileDataTable(DataTable result, string fileName)
        {
            if (result == null || String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Result or fileName cannot be empty.");
            }
            fileName = string.Format("{1}-{0}", DateTime.Now.ToString("yyyy-MM-dd"), fileName);
            if (result.Rows.Count < 65534)
            {
                var ms = ExcelHelper.GetExcelByteArrayFromDataTable(result);
                return File(ms, "application/vnd.ms-excel", fileName + ".xls");
            }
            else
            {
                byte[] data = ExcelHelper.Export(result, true);
                return File(data, "text/csv", fileName + ".csv");
            }
        }

        protected void RemoveModelState()
        {
            RemoveModelState("Id");
            RemoveModelState("CreatedDate");
            RemoveModelState("UpdatedDate");
            RemoveModelState("Lang");
        }

        private void RemoveModelState(string key)
        {
            if (ModelState.ContainsKey(key))
            {
                ModelState.Remove(key);
            }
        }
    }
}