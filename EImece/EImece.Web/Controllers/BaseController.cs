using EImece.Domain;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using EImece.Web.Filters;
using Microsoft.Extensions.Logging;
using Resources;
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EImece.Web.Controllers
{
    /// <summary>
    /// Storefront base controller. [TimedActionFilter] auto-derives a per-action histogram
    /// "app.{controller}.{action}" (ms) so all storefront actions are measured.
    /// Overall HTTP duration remains via OpenTelemetry.Instrumentation.AspNet.
    /// </summary>
    [TimedActionFilter]
    [UnderConst]
    public abstract class BaseController : Controller
    {
        protected readonly ISettingService SettingService;
        protected readonly AutoMapper.IMapper Mapper;
        protected readonly ILogger Logger;

        protected BaseController(ISettingService settingService, AutoMapper.IMapper mapper, ILogger logger)
        {
            SettingService = settingService;
            Mapper = mapper;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void CreateLanguageCookie(EImeceLanguage selectedLanguage, string cookieName)
        {
            String cultureName = EnumHelper.GetEnumDescription(selectedLanguage);
            var culture = CultureInfo.GetCultureInfo(cultureName);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Resource.Culture = culture;
            AdminResource.Culture = culture;
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
                Logger.LogError("OnException:" + filterContext.Exception.ToFormattedString());
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
                    ContentType = Constants.TextHtmlUtf8ContentType,
                    Content = BuildDetailedErrorHtml(filterContext)
                };
                return;
            }

            base.OnException(filterContext);
        }

        protected ActionResult HandleUnexpectedError(Exception exception, string contextMessage)
        {
            Logger.LogError(exception, contextMessage);
            if (ShouldExposeDetailedErrors(HttpContext))
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
            }

            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;

            var model = new EImece.Domain.Models.HelperModels.ErrorModel
            {
                RequestedUrl = Request?.Url != null ? Request.Url.ToString() : string.Empty,
                ReferrerUrl = Request?.UrlReferrer != null ? Request.UrlReferrer.ToString() : null
            };

            ViewBag.Title = Resources.Resource.UnexpectedErrorText;
            ViewBag.Description = Resources.Resource.InternalServererrorText;
            ViewBag.ExceptionDetail = exception;

            return View("~/Views/Error/InternalServerError.cshtml", model);
        }

        /// <summary>
        /// Renders the 404 Not Found error view directly with HTTP 404 response code without redirecting.
        /// </summary>
        protected ActionResult HttpNotFoundView(string message = null)
        {
            Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
            Response.TrySkipIisCustomErrors = true;

            var model = new EImece.Domain.Models.HelperModels.ErrorModel
            {
                RequestedUrl = Request?.Url != null ? Request.Url.ToString() : string.Empty,
                ReferrerUrl = Request?.UrlReferrer != null ? Request.UrlReferrer.ToString() : null
            };

            ViewBag.Title = Resources.Resource.PageNotFoundText;
            ViewBag.Description = !string.IsNullOrWhiteSpace(message) ? message : Resources.Resource.NotFoundText;

            if (Request != null && Request.IsAjaxRequest())
            {
                return PartialView("~/Views/Error/NotFound.cshtml", model);
            }

            return View("~/Views/Error/NotFound.cshtml", model);
        }

        /// <summary>
        /// Renders the 410 Gone error view directly with HTTP 410 response code for inactive/deleted entities without redirecting.
        /// </summary>
        protected ActionResult HttpGoneView(string message = null)
        {
            Response.StatusCode = (int)System.Net.HttpStatusCode.Gone; // 410
            Response.TrySkipIisCustomErrors = true;

            var model = new EImece.Domain.Models.HelperModels.ErrorModel
            {
                RequestedUrl = Request?.Url != null ? Request.Url.ToString() : string.Empty,
                ReferrerUrl = Request?.UrlReferrer != null ? Request.UrlReferrer.ToString() : null
            };

            ViewBag.Title = Resources.Resource.PageNotFoundText;
            ViewBag.Description = !string.IsNullOrWhiteSpace(message) ? message : Resources.Resource.NotFoundText;

            if (Request != null && Request.IsAjaxRequest())
            {
                return PartialView("~/Views/Error/Gone.cshtml", model);
            }

            return View("~/Views/Error/Gone.cshtml", model);
        }

        protected static bool ShouldExposeDetailedErrors(HttpContextBase httpContext)
        {
            // compilation debug="true" in Web.config
            return httpContext != null && httpContext.IsDebuggingEnabled;
        }

        private static string BuildDetailedErrorHtml(ExceptionContext filterContext)
        {
            var ex = filterContext.Exception;
            var controller = filterContext.RouteData != null && filterContext.RouteData.Values != null
                ? filterContext.RouteData.Values["controller"]
                : null;
            var action = filterContext.RouteData != null && filterContext.RouteData.Values != null
                ? filterContext.RouteData.Values["action"]
                : null;
            var url = filterContext.HttpContext != null && filterContext.HttpContext.Request != null
                ? filterContext.HttpContext.Request.Url
                : null;

            var sb = new StringBuilder(1024);
            sb.Append("<!DOCTYPE html><html><head><title>Error</title>")
              .Append("<style>body{font-family:Consolas,monospace;padding:24px;background:#111;color:#f5f5f5}")
              .Append("h1{color:#ff6b6b}pre{white-space:pre-wrap;background:#1e1e1e;padding:16px;border-radius:8px}</style>")
              .Append("</head><body>")
              .Append("<h1>Unhandled exception</h1>")
              .Append("<p><b>URL:</b> ").Append(HttpUtility.HtmlEncode(url != null ? url.ToString() : string.Empty)).Append("</p>")
              .Append("<p><b>Controller:</b> ").Append(HttpUtility.HtmlEncode(System.Convert.ToString(controller)))
              .Append(" &nbsp; <b>Action:</b> ").Append(HttpUtility.HtmlEncode(System.Convert.ToString(action))).Append("</p>")
              .Append("<p><b>Type:</b> ").Append(HttpUtility.HtmlEncode(ex != null ? ex.GetType().FullName : string.Empty)).Append("</p>")
              .Append("<p><b>Message:</b> ").Append(HttpUtility.HtmlEncode(ex != null ? ex.Message : string.Empty)).Append("</p>")
              .Append("<h2>Stack trace</h2><pre>").Append(HttpUtility.HtmlEncode(ex != null ? ex.ToString() : string.Empty)).Append("</pre>")
              .Append("</body></html>");

            return sb.ToString();
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            var languageCookie = requestContext?.HttpContext?.Request?.Cookies["Language"];
            var cultureCookie = requestContext?.HttpContext?.Request?.Cookies[Constants.CultureCookieName];
            var cultureName = ContentLanguageSettingsHelper.ResolveStorefrontCulture(
                languageCookie != null ? languageCookie.Value : null,
                cultureCookie != null ? cultureCookie.Values[Constants.ELanguage] : null);
            SetCurrentCulture(cultureName);

            base.Initialize(requestContext);
        }

        protected bool IsProductPriceEnabled
        {
            get
            {
                if (SettingService == null)
                    return true;
                var setting = SettingService.GetSettingByKey(Constants.IsProductPriceEnable);
                return setting.ToBool(true);
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext != null && filterContext.HttpContext != null)
            {
                var controller = filterContext.ActionDescriptor?.ControllerDescriptor?.ControllerName ?? string.Empty;
                var action = filterContext.ActionDescriptor?.ActionName ?? string.Empty;
                var area = filterContext.RouteData?.DataTokens["area"]?.ToString() ?? string.Empty;

                if (!controller.Equals("UnderConstruction", StringComparison.OrdinalIgnoreCase) &&
                    !controller.Equals("Error", StringComparison.OrdinalIgnoreCase) &&
                    !area.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
                    !(controller.Equals("Account", StringComparison.OrdinalIgnoreCase) &&
                      (action.Equals("AdminLogin", StringComparison.OrdinalIgnoreCase) ||
                       action.Equals("VerifyAuthenticator", StringComparison.OrdinalIgnoreCase) ||
                       action.Equals("LogOff", StringComparison.OrdinalIgnoreCase))))
                {
                    var settingService = SettingService;
                    if (settingService == null)
                    {
                        try
                        {
                            settingService = DependencyResolver.Current?.GetService(typeof(ISettingService)) as ISettingService;
                        }
                        catch
                        {
                            settingService = null;
                        }
                    }

                    var isUnderConstruction = settingService != null
                        ? settingService.GetSettingByKey(Constants.IsSiteUnderConstruction).ToBool(false)
                        : false;

                    if (isUnderConstruction)
                    {
                        var user = filterContext.HttpContext.User;
                        var isAuth = user != null && user.Identity != null && user.Identity.IsAuthenticated;
                        var isAdmin = isAuth && (user.IsInRole(Constants.AdministratorRole) || user.IsInRole(Constants.EditorRole));

                        if (!isAdmin)
                        {
                            if (filterContext.IsChildAction)
                            {
                                filterContext.Result = new EmptyResult();
                                return;
                            }

                            filterContext.Result = new RedirectResult("/underconstruction");
                            return;
                        }
                    }
                }
            }

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
            SetCurrentCulture(language + "");
        }

        protected void SetCurrentCulture(String language)
        {
            if (String.IsNullOrEmpty(language))
                return;

            var cultureName = language.Trim();
            int langId;
            if (int.TryParse(cultureName, out langId) && Enum.IsDefined(typeof(EImeceLanguage), langId))
            {
                cultureName = EnumHelper.GetEnumDescription((EImeceLanguage)langId);
            }
            else if (!cultureName.Contains("-"))
            {
                var langEnum = EnumHelper.ParseLanguage(cultureName);
                if (langEnum.HasValue)
                {
                    cultureName = EnumHelper.GetEnumDescription(langEnum.Value);
                }
            }

            CultureInfo culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(cultureName);
            }
            catch
            {
                culture = CultureInfo.GetCultureInfo(Constants.TR);
            }

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Resource.Culture = culture;
            AdminResource.Culture = culture;
        }

        protected void SetLanguage(string id)
        {
            var contentLanguages = ContentLanguageSettingsHelper.GetCurrent();
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

            if (!contentLanguages.IsLanguageEnabled(selectedLanguage))
            {
                selectedLanguage = contentLanguages.DefaultLanguage;
            }

            String cultureName = EnumHelper.GetEnumDescription(selectedLanguage);
            var culture = CultureInfo.GetCultureInfo(cultureName);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Resource.Culture = culture;
            AdminResource.Culture = culture;

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