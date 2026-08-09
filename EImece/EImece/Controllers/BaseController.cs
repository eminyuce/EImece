using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using EImece.Domain.DependencyInjection;
using NLog;
using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [UnderConst]
    public abstract class BaseController : Controller
    {
        [Inject]
        public ISettingService SettingService { get; set; }

        private static readonly Logger BaseLogger = LogManager.GetCurrentClassLogger();

        public void CreateLanguageCookie(EImeceLanguage selectedLanguage, string cookieName)
        {
            String cultureName = EnumHelper.GetEnumDescription(selectedLanguage);
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
        }

        protected int CurrentLanguage
        {
            get
            {
                var lang = Thread.CurrentThread.CurrentCulture.ToString();
                return EnumHelper.GetEnumFromDescription(lang, typeof(EImeceLanguage));
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.Exception != null)
            {
                BaseLogger.Error("OnException:" + filterContext.Exception.ToFormattedString());
            }

            var exposeDetails = ShouldExposeDetailedErrors(filterContext != null ? filterContext.HttpContext : null);

            if (filterContext != null
                && !filterContext.ExceptionHandled
                && exposeDetails
                && filterContext.Exception != null)
            {
                // Surface full stack instead of empty/generic 500 pages while debugging.
                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();
                filterContext.HttpContext.Response.StatusCode = 500;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                filterContext.HttpContext.Response.Cache.SetNoStore();
                filterContext.Result = new ContentResult
                {
                    ContentType = "text/html; charset=utf-8",
                    Content = BuildDetailedErrorHtml(filterContext)
                };
                return;
            }

            base.OnException(filterContext);
        }

        protected ActionResult HandleUnexpectedError(Exception exception, string contextMessage)
        {
            BaseLogger.Error(exception, contextMessage);
            if (ShouldExposeDetailedErrors(HttpContext))
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
            }

            TempData["LastException"] = exception;
            return RedirectToAction("InternalServerError", "Error");
        }

        protected static bool ShouldExposeDetailedErrors(HttpContextBase httpContext)
        {
            if (AppConfig.ExposeDetailedErrors)
            {
                return true;
            }

            // compilation debug="true" in Web.config
            return httpContext != null && httpContext.IsDebuggingEnabled;
        }

        private static string BuildDetailedErrorHtml(ExceptionContext filterContext)
        {
            var ex = filterContext.Exception;
            var controller = filterContext.RouteData.Values["controller"];
            var action = filterContext.RouteData.Values["action"];
            var url = filterContext.HttpContext.Request.Url;
            return
                "<!DOCTYPE html><html><head><title>Error</title>" +
                "<style>body{font-family:Consolas,monospace;padding:24px;background:#111;color:#f5f5f5}" +
                "h1{color:#ff6b6b}pre{white-space:pre-wrap;background:#1e1e1e;padding:16px;border-radius:8px}</style>" +
                "</head><body>" +
                "<h1>Unhandled exception</h1>" +
                "<p><b>URL:</b> " + HttpUtility.HtmlEncode(url != null ? url.ToString() : "") + "</p>" +
                "<p><b>Controller:</b> " + HttpUtility.HtmlEncode(System.Convert.ToString(controller)) +
                " &nbsp; <b>Action:</b> " + HttpUtility.HtmlEncode(System.Convert.ToString(action)) + "</p>" +
                "<p><b>Type:</b> " + HttpUtility.HtmlEncode(ex.GetType().FullName) + "</p>" +
                "<p><b>Message:</b> " + HttpUtility.HtmlEncode(ex.Message) + "</p>" +
                "<h2>Stack trace</h2><pre>" + HttpUtility.HtmlEncode(ex.ToString()) + "</pre>" +
                "</body></html>";
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            HttpCookie languageCookie = System.Web.HttpContext.Current.Request.Cookies["Language"];
            if (languageCookie != null)
            {
                SetCurrentCulture(languageCookie.Value);
            }
            else
            {
                //other code here
            }

            base.Initialize(requestContext);
        }

        protected bool IsProductPriceEnabled
        {
            get
            {
                return SettingService.GetSettingByKey(Constants.IsProductPriceEnable).ToBool(true);
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.IsProductPriceEnable = IsProductPriceEnabled;
            base.OnActionExecuting(filterContext);
        }

        protected void SetCurrentCulture(BaseEntity entity)
        {
            if (entity == null)
                return;
            SetCurrentCulture(entity.Lang);
        }

        protected void SetCurrentCulture(int language)
        {
            if (language == 0)
                return;
            SetLanguage(language + "");
        }

        protected void SetCurrentCulture(String language)
        {
            if (String.IsNullOrEmpty(language))
                return;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
        }

        protected void SetLanguage(string id)
        {
            // Accept either enum numeric id ("1") or culture description ("tr-TR").
            EImeceLanguage selectedLanguage;
            int langId;
            if (int.TryParse(id, out langId) && Enum.IsDefined(typeof(EImeceLanguage), langId))
            {
                selectedLanguage = (EImeceLanguage)langId;
            }
            else
            {
                selectedLanguage = (EImeceLanguage)EnumHelper.GetEnumFromDescription(id, typeof(EImeceLanguage));
            }

            String cultureName = EnumHelper.GetEnumDescription(selectedLanguage);
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);

            CreateLanguageCookie(selectedLanguage, Constants.CultureCookieName);

            Response.Cookies.Remove("Language");

            var languageCookie = System.Web.HttpContext.Current.Request.Cookies["Language"];

            if (languageCookie == null) languageCookie = new HttpCookie("Language");

            languageCookie.Value = cultureName;

            languageCookie.Expires = DateTime.Now.AddDays(10);

            Response.SetCookie(languageCookie);
        }
    }
}