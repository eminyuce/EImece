using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Observability.Logging;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NLog;
using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Griddly.Mvc;
using DomainConstants = EImece.Domain.Constants;

namespace EImece.Areas.Admin.Controllers
{
    /// <summary>
    /// Lightweight base controller for all Admin controllers.
    /// Provides shared MVC lifecycle handling (exception logging, action filters,
    /// localization, 2FA enforcement, and view/grid helpers) without acting as a "god object".
    /// Pure constructor injection is enforced; no optional parameters or ServiceLocator fallbacks.
    /// </summary>
    [AuthorizeRoles(DomainConstants.AdministratorRole, DomainConstants.EditorRole)]
    public abstract class BaseAdminController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected ISettingService SettingService { get; }

        protected BaseAdminController(ISettingService settingService)
        {
            SettingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
        }

        #region Exception & Lifecycle Filters

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
                        Content = $"<!DOCTYPE html><html><head><title>Admin Error</title><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><style>body{{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;padding:32px;background:#f8f9fa;color:#212529}}h1{{color:#dc3545;font-size:22px;margin-top:0}}.card{{background:#fff;border:1px solid #dee2e6;border-radius:8px;padding:24px;max-width:800px;margin:20px 0;box-shadow:0 2px 4px rgba(0,0,0,0.05)}}.ref{{font-family:monospace;color:#6c757d;font-size:13px;margin-top:12px}}pre{{background:#f1f1f1;padding:10px;overflow:auto;}}</style></head><body><div class=\"card\"><h1>Admin Error</h1><p>An unexpected error occurred in the administration panel.</p><p class=\"ref\">Correlation ID: {HttpUtility.HtmlEncode(correlationId)}</p><p><strong>Exception:</strong> {HttpUtility.HtmlEncode(ex.Message)}</p><pre>{HttpUtility.HtmlEncode(ex.ToString())}</pre><a href=\"/admin\" style=\"display:inline-block;margin-top:16px;color:#0d6efd;text-decoration:none\">&larr; Return to Admin Dashboard</a></div></body></html>"
                    };
                }
            }

            base.OnException(filterContext);
        }

        private static readonly HashSet<string> PriceRelatedAdminControllers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Coupons",
            "Orders",
            "Customers",
            "ShoppingCarts",
            "Report"
        };

        private static readonly HashSet<string> ReviewRelatedAdminControllers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ProductComments"
        };

        protected bool IsProductPriceEnabled =>
            SettingService.GetSettingByKey(DomainConstants.IsProductPriceEnable).ToBool(true);

        protected bool IsProductReviewEnabled =>
            SettingService.GetSettingByKey(DomainConstants.IsProductReviewEnable).ToBool(true);

        private static void SetGriddlyDefaultPageSize(int pageSize)
        {
            GriddlySettings.DefaultPageSize = pageSize;
        }

        private static void SetDefaultCultures(CultureInfo culture)
        {
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var uiLang = AdminPanelUILanguage;
            var cultureName = uiLang == 2 ? "en-US" : "tr-TR";
            var culture = CultureInfo.GetCultureInfo(cultureName);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            SetDefaultCultures(culture);
            AdminResource.Culture = culture;
            Resource.Culture = culture;

            ViewBag.IsProductPriceEnable = SettingService.GetSettingObjectByKey(DomainConstants.IsProductPriceEnable);
            ViewBag.IsProductReviewEnable = SettingService.GetSettingObjectByKey(DomainConstants.IsProductReviewEnable);
            int gridPageSize = SettingService.GetSettingByKey(DomainConstants.GridPageSizeNumber).ToInt(DomainConstants.DefaultGridPageSizeNumber);
            ViewBag.GridPageSizeNumber = gridPageSize;
            SetGriddlyDefaultPageSize(gridPageSize);
            ViewBag.CurrentLanguage = CurrentLanguage;

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

            if (!IsProductReviewEnabled)
            {
                var controllerName = filterContext.RouteData.Values["controller"] as string ?? string.Empty;
                if (ReviewRelatedAdminControllers.Contains(controllerName))
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

        private bool MustRedirectToEnableAuthenticator(ActionExecutingContext filterContext)
        {
            bool requireAuth = SettingService.GetSettingByKey(DomainConstants.RequireAdminAuthenticator).ToBool(DomainConstants.DefaultRequireAdminAuthenticator);
            if (!requireAuth)
            {
                return false;
            }

            if (filterContext.IsChildAction)
            {
                return false;
            }

            var httpContext = filterContext.HttpContext;
            if (httpContext?.User?.Identity == null || !httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            var userManager = httpContext.GetOwinContext()?.GetUserManager<ApplicationUserManager>();
            if (userManager == null)
            {
                return false;
            }

            var controllerName = filterContext.RouteData.Values["controller"] as string ?? string.Empty;
            var actionName = filterContext.RouteData.Values["action"] as string ?? string.Empty;

            if (IsAuthenticatorSetupOrLogOff(controllerName, actionName))
            {
                return false;
            }

            var userId = httpContext.User.Identity.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var user = userManager.FindById(userId);
            if (user == null)
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

        #endregion

        #region Language & Localization

        protected int SelectedLanguage
        {
            get
            {
                if (Session?[DomainConstants.SelectedLanguage] != null)
                {
                    return Session[DomainConstants.SelectedLanguage].ToInt(1);
                }
                return AppConfig.MainLanguage;
            }
            set
            {
                if (Session != null)
                {
                    Session[DomainConstants.SelectedLanguage] = value;
                }
            }
        }

        public EImeceLanguage GetCurrentLanguage => (EImeceLanguage)CurrentLanguage;

        public int CurrentLanguage
        {
            get
            {
                var contentLanguages = ContentLanguageSettingsHelper.GetCurrent();
                if (!contentLanguages.IsBilingual)
                {
                    return contentLanguages.DefaultLanguageId;
                }

                HttpCookie cultureCookie = Request?.Cookies != null ? Request.Cookies[DomainConstants.AdminCultureCookieName] : null;
                if (cultureCookie != null)
                {
                    var cookieLang = cultureCookie.Values[DomainConstants.ELanguage].ToInt();
                    if (contentLanguages.IsLanguageEnabled((EImeceLanguage)cookieLang))
                    {
                        return cookieLang;
                    }
                }
                return contentLanguages.DefaultLanguageId;
            }
        }

        protected int AdminPanelUILanguage
        {
            get
            {
                var settingValue = SettingService.GetSettingByKey(DomainConstants.AdminPanelLanguage);
                if (!string.IsNullOrWhiteSpace(settingValue))
                {
                    var langEnum = EnumHelper.ParseLanguage(settingValue);
                    if (langEnum.HasValue)
                    {
                        return (int)langEnum.Value;
                    }
                }
                return DomainConstants.DefaultAdminPanelLanguage == "en-US" ? 2 : 1;
            }
        }

        #endregion

        #region Shared Action Helpers & Results

        protected bool CanRenderGrid()
        {
            if (ControllerContext != null && ControllerContext.IsChildAction) return true;
            if (Request != null && Request.IsAjaxRequest()) return true;
            if (Request?.Headers != null && !string.IsNullOrEmpty(Request.Headers["X-Requested-With"])) return true;
            return string.Equals(Request?["gridembed"], "1", StringComparison.OrdinalIgnoreCase);
        }

        protected ActionResult RequestReturn(RedirectToRouteResult returnDefault)
        {
            string redirectUrl;
            if (SecurityHelper.TryGetSafeReferrerRedirect(Request?.UrlReferrer, Request?.Url, out redirectUrl))
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

        protected ActionResult ReturnIndexIfNotUrlReferrer(string action)
        {
            string redirectUrl;
            if (Request?.UrlReferrer == null || Request.UrlReferrer.ToStr().ToLowerInvariant().Contains("saveoredit"))
            {
                return RedirectToAction(action);
            }
            if (SecurityHelper.TryGetSafeReferrerRedirect(Request.UrlReferrer, Request.Url, out redirectUrl))
            {
                return Redirect(redirectUrl);
            }

            return RedirectToAction(action);
        }

        protected ActionResult ReturnIndexIfNotUrlReferrer(string action, object routeValues)
        {
            string redirectUrl;
            if (Request?.UrlReferrer == null || Request.UrlReferrer.ToStr().ToLowerInvariant().Contains("saveoredit"))
            {
                return RedirectToAction(action, routeValues);
            }
            if (SecurityHelper.TryGetSafeReferrerRedirect(Request.UrlReferrer, Request.Url, out redirectUrl))
            {
                return Redirect(redirectUrl);
            }

            return RedirectToAction(action, routeValues);
        }

        protected ActionResult DownloadFileDataTable(DataTable result, string fileName, string format = "excel")
        {
            if (result == null || string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("Result or fileName cannot be empty.");
            }
            fileName = string.Format("{1}-{0}", DateTime.Now.ToString("yyyy-MM-dd"), fileName);

            var isCsv = string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase);
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

        #endregion
    }
}