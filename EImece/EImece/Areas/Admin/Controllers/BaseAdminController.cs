using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Factories.IFactories;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using Microsoft.AspNet.Identity;
using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DomainConstants = EImece.Domain.Constants;
using NLog;
using EImece.Domain.Observability.Logging;

namespace EImece.Areas.Admin.Controllers
{
    [AuthorizeRoles(DomainConstants.AdministratorRole, DomainConstants.EditorRole)]
    public abstract class BaseAdminController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IEntityFactory EntityFactory { get; set; }

        [Inject]
        public ICouponService CouponService { get; set; }

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
        public IEimeceCacheProvider MemoryCacheProvider { get; set; }

        [Inject]
        public IMailTemplateService MailTemplateService { get; set; }

        [Inject]
        public IOrderService OrderService { get; set; }

        [Inject]
        public IOrderProductService OrderProductService { get; set; }

        [Inject]
        public IFaqService FaqService { get; set; }

        [Inject]
        public ApplicationUserManager UserManager { get; set; }

        private FilesHelper _filesHelper { get; set; }

        public BaseAdminController()
        {
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null || filterContext.Exception == null)
            {
                base.OnException(filterContext);
                return;
            }

            var ex = filterContext.Exception;
            var controller = filterContext.RouteData?.Values["controller"]?.ToString() ?? "";
            var action = filterContext.RouteData?.Values["action"]?.ToString() ?? "";
            var correlationId = CorrelationIdContext.Current ?? CorrelationIdContext.Ensure();

            Logger.Error(ex, "Admin exception in {0}/{1} (CorrelationId: {2}): {3}", controller, action, correlationId, ex.Message);

            if (!filterContext.ExceptionHandled)
            {
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;

                bool isAjax = filterContext.HttpContext.Request.IsAjaxRequest() ||
                              (filterContext.HttpContext.Request.AcceptTypes != null &&
                               filterContext.HttpContext.Request.AcceptTypes.Any(t => t.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0));

                if (isAjax)
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            success = false,
                            message = "An error occurred while processing the admin request.",
                            correlationId = correlationId
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    filterContext.Result = new ContentResult
                    {
                        ContentType = "text/html; charset=utf-8",
                        Content = $"<!DOCTYPE html><html><head><title>Admin Error</title><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><style>body{{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;padding:32px;background:#f8f9fa;color:#212529}}h1{{color:#dc3545;font-size:22px;margin-top:0}}.card{{background:#fff;border:1px solid #dee2e6;border-radius:8px;padding:24px;max-width:600px;margin:20px 0;box-shadow:0 2px 4px rgba(0,0,0,0.05)}}.ref{{font-family:monospace;color:#6c757d;font-size:13px;margin-top:12px}}</style></head><body><div class=\"card\"><h1>Admin Error</h1><p>An unexpected error occurred in the administration panel.</p><p class=\"ref\">Correlation ID: {HttpUtility.HtmlEncode(correlationId)}</p><a href=\"/admin\" style=\"display:inline-block;margin-top:16px;color:#0d6efd;text-decoration:none\">&larr; Return to Admin Dashboard</a></div></body></html>"
                    };
                }
            }

            base.OnException(filterContext);
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

        private static readonly HashSet<string> PriceRelatedAdminControllers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Coupons",
            "Orders",
            "Customers",
            "ShoppingCarts",
            "Report"
        };

        protected bool IsProductPriceEnabled
        {
            get
            {
                return SettingService.GetSettingByKey(DomainConstants.IsProductPriceEnable).ToBool(true);
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.IsProductPriceEnable = SettingService.GetSettingObjectByKey(DomainConstants.IsProductPriceEnable);

            if (!IsProductPriceEnabled)
            {
                var controllerName = filterContext.RouteData.Values["controller"] as string ?? string.Empty;
                if (PriceRelatedAdminControllers.Contains(controllerName))
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { area = "Admin", controller = "Dashboard", action = "Index" }));
                    return;
                }
            }

            if (MustRedirectToEnableAuthenticator(filterContext))
            {
                TempData[DomainConstants.StatusMessageKey] = "Yönetici paneline devam etmek için Authenticator 2FA etkinleştirmeniz zorunludur.";
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { area = "Admin", controller = "Users", action = "EnableAuthenticator" }));
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// Forces Authenticator setup for admin/editor users when required.
        /// Skipped for: compilation debug, BypassAdminAuth, config off, bypass users, or the setup actions themselves.
        /// </summary>
        private bool MustRedirectToEnableAuthenticator(ActionExecutingContext filterContext)
        {
            if (!AppConfig.RequireAdminAuthenticator || AppConfig.BypassAdminAuth)
            {
                return false;
            }

            // Child actions (e.g. Html.Action in _AdminTopbar) cannot redirect.
            if (filterContext.IsChildAction)
            {
                return false;
            }

            var httpContext = filterContext.HttpContext;
            if (httpContext == null || httpContext.IsDebuggingEnabled)
            {
                return false;
            }

            var controllerName = filterContext.RouteData.Values["controller"] as string ?? string.Empty;
            var actionName = filterContext.RouteData.Values["action"] as string ?? string.Empty;

            if (IsAuthenticatorSetupOrLogOff(controllerName, actionName))
            {
                return false;
            }

            if (httpContext.User == null || httpContext.User.Identity == null || !httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            if (UserManager == null)
            {
                return false;
            }

            var userId = httpContext.User.Identity.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var user = UserManager.FindById(userId);
            if (user == null)
            {
                return false;
            }

            if (AppConfig.IsTwoFactorBypassUser(user.Email) || AppConfig.IsTwoFactorBypassUser(user.UserName))
            {
                return false;
            }

            return !user.TwoFactorAuthenticatorEnabled;
        }

        private static bool IsAuthenticatorSetupOrLogOff(string controllerName, string actionName)
        {
            if (string.Equals(actionName, "LogOff", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(controllerName, "Users", StringComparison.OrdinalIgnoreCase)
                && (string.Equals(actionName, "EnableAuthenticator", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(actionName, "DisableAuthenticator", StringComparison.OrdinalIgnoreCase));
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
                if (Session[DomainConstants.SelectedLanguage] != null)
                {
                    return Session[DomainConstants.SelectedLanguage].ToInt(1);
                }
                else
                {
                    return AppConfig.MainLanguage;
                }
            }
            set
            {
                Session[DomainConstants.SelectedLanguage] = value;
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
                    HttpCookie cultureCookie = Request.Cookies[DomainConstants.AdminCultureCookieName];
                    if (cultureCookie != null)
                    {
                        return cultureCookie.Values[DomainConstants.ELanguage].ToInt();
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
            string redirectUrl;
            if (SecurityHelper.TryGetSafeReferrerRedirect(Request.UrlReferrer, Request.Url, out redirectUrl))
            {
                return Redirect(redirectUrl);
            }

            return returnDefault;
        }

        protected ActionResult DownloadFile<T>(IEnumerable<T> result, string fileName, string format = "excel")
        {
            DataTable dt = GeneralHelper.LINQToDataTable(result);
            dt.TableName = fileName;
            return DownloadFileDataTable(dt, fileName, format);
        }

        protected ActionResult ReturnIndexIfNotUrlReferrer(String action)
        {
            string redirectUrl;
            if (Request.UrlReferrer == null || Request.UrlReferrer.ToStr().ToLowerInvariant().Contains("saveoredit"))
            {
                return RedirectToAction(action);
            }
            else if (SecurityHelper.TryGetSafeReferrerRedirect(Request.UrlReferrer, Request.Url, out redirectUrl))
            {
                return Redirect(redirectUrl);
            }

            return RedirectToAction(action);
        }

        protected ActionResult ReturnIndexIfNotUrlReferrer(String action, object routeValues)
        {
            string redirectUrl;
            if (Request.UrlReferrer == null || Request.UrlReferrer.ToStr().ToLowerInvariant().Contains("saveoredit"))
            {
                return RedirectToAction(action, routeValues);
            }
            else if (SecurityHelper.TryGetSafeReferrerRedirect(Request.UrlReferrer, Request.Url, out redirectUrl))
            {
                return Redirect(redirectUrl);
            }

            return RedirectToAction(action, routeValues);
        }

        protected ActionResult DownloadFileDataTable(DataTable result, string fileName, string format = "excel")
        {
            if (result == null || String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Result or fileName cannot be empty.");
            }
            fileName = string.Format("{1}-{0}", DateTime.Now.ToString("yyyy-MM-dd"), fileName);

            var isCsv = string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase);
            // HSSF (.xls) supports at most 65536 rows; fall back to CSV when Excel cannot fit the data.
            var useCsv = isCsv || result.Rows.Count >= 65534;

            if (useCsv)
            {
                byte[] data = ExcelHelper.Export(result, true);
                return File(data, "text/csv", fileName + ".csv");
            }

            var ms = ExcelHelper.GetExcelByteArrayFromDataTable(result);
            return File(ms, "application/vnd.ms-excel", fileName + ".xls");
        }

        protected void RemoveModelState()
        {
            RemoveModelState("Id");
            RemoveModelState("CreatedDate");
            RemoveModelState("UpdatedDate");
            RemoveModelState("Lang");
        }

        protected void SetSuccessMessage(string message = null)
        {
            TempData[DomainConstants.StatusMessageKey] = string.IsNullOrEmpty(message)
                ? AdminResource.SuccessfullySavedCompleted
                : message;
            TempData["StatusMessageType"] = "success";
        }

        protected void SetErrorMessage(string message = null)
        {
            TempData[DomainConstants.StatusMessageKey] = string.IsNullOrEmpty(message)
                ? AdminResource.GeneralSaveErrorMessage
                : message;
            TempData["StatusMessageType"] = "danger";
        }

        protected void SetStatusMessage(string message, string messageType)
        {
            TempData[DomainConstants.StatusMessageKey] = message;
            TempData["StatusMessageType"] = string.IsNullOrEmpty(messageType) ? "info" : messageType;
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