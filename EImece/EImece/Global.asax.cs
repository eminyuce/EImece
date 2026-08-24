using EImece.App_Start;
using EImece.Controllers;
using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Microsoft.AspNet.Identity;
using NLog;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace EImece
{
    /// <summary>
    /// The main HTTP application class that handles application lifecycle events, 
    /// configuration, and early request pipeline processing (such as canonical URL redirects).
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            // Fail closed on missing/placeholder DB credentials before any request handling.
            ConnectionStringProvider.Initialize();
            DependencyInjectionConfig.Register();

            //System.Net.ServicePointManager.SecurityProtocol
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // TLS 1.2 only; older protocols (TLS 1.0/1.1) removed for security

            ViewEngineConfig.RegisterViewEngines(ViewEngines.Engines);

            string activeDesign = new EImece.Infrastructure.Designs.ConfigDesignProvider().GetActiveDesign();
            bool validateOnStartup = string.Equals(System.Configuration.ConfigurationManager.AppSettings["ValidateDesignOnStartup"], "true", StringComparison.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(activeDesign) && validateOnStartup)
            {
                EImece.Infrastructure.Designs.DesignValidator.EnsureValidDesign(activeDesign);
            }

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Required for AntiForgeryToken with claims-based auth.
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            MvcHandler.DisableMvcResponseHeader = true;

            ObservabilityBootstrap.Configure();
            using (DependencyInjectionConfig.BeginAmbientScope())
            {
                var metrics = DependencyResolver.Current.GetService<EImece.Domain.Observability.Metrics.IApplicationMetrics>();
                var observabilityOptions = DependencyResolver.Current.GetService<EImece.Domain.Observability.Configuration.ObservabilityOptions>()
                    ?? EImece.Domain.Observability.Configuration.ObservabilityOptions.FromAppConfig();
                GlobalFilters.Filters.Add(new Filters.TelemetryActionFilter(metrics, observabilityOptions));
                GlobalFilters.Filters.Add(new Filters.StructuredExceptionFilter());

                var adresService = DependencyResolver.Current.GetService<AdresService>();
            }

            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.DependencyResolver =
                new MsDiWebApiDependencyResolver(DependencyInjectionConfig.ServiceProvider);

            // Start Quartz background scheduler services if enabled with resilient AppDomain protection
            try
            {
                var adminQuartzService = DependencyResolver.Current.GetService<EImece.Domain.Scheduler.AdminQuartzService>();
                if (adminQuartzService != null)
                {
                    HostingEnvironment.QueueBackgroundWorkItem(async ct =>
                    {
                        try
                        {
                            await adminQuartzService.StartSchedulerServiceAsync().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Unhandled exception during AdminQuartzService background execution.");
                        }
                    });
                }

                var userQuartzService = DependencyResolver.Current.GetService<EImece.Domain.Scheduler.UserQuartzService>();
                if (userQuartzService != null)
                {
                    HostingEnvironment.QueueBackgroundWorkItem(async ct =>
                    {
                        try
                        {
                            await userQuartzService.StartSchedulerServiceAsync().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Unhandled exception during UserQuartzService background execution.");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to initialize Quartz scheduler services.");
            }
        }

        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            if (custom == "User")
            {
                HttpCookie cultureCookie = Request.Cookies[Domain.Constants.CultureCookieName];
                String cultureCookieValue = "";
                if (cultureCookie != null)
                {
                    cultureCookieValue = cultureCookie.Values[Domain.Constants.ELanguage].ToStr();
                }

                if (User.Identity.IsAuthenticated)
                {
                    return string.Format("User:{0}:Lang:{1}",
                        context.User.Identity.Name,
                        cultureCookieValue);
                }
                else
                {
                    return string.Format("Anon:Lang:{0}", cultureCookieValue);
                }
            }

            return base.GetVaryByCustomString(context, custom);
        }

        /// <summary>
        /// Occurs as the first event in the HTTP pipeline chain of execution when ASP.NET responds to a request.
        /// Invokes canonical URL enforcement and maintenance mode redirection.
        /// </summary>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            DependencyInjectionConfig.BeginRequestScope();
            Redirect301();
            EnforceUnderConstructionRedirect();
        }

        private void EnforceUnderConstructionRedirect()
        {
            try
            {
                if (Context == null || Context.Request == null)
                {
                    return;
                }

                var path = (Context.Request.AppRelativeCurrentExecutionFilePath ?? Context.Request.Path ?? "")
                    .TrimStart('~')
                    .ToLowerInvariant();

                // Skip static assets, bundles, media, and images
                if (path.StartsWith("/content/") ||
                    path.StartsWith("/scripts/") ||
                    path.StartsWith("/bundles/") ||
                    path.StartsWith("/media/") ||
                    path.StartsWith("/images/") ||
                    path.StartsWith("/fonts/") ||
                    path.StartsWith("/favicon") ||
                    path.EndsWith(".css") ||
                    path.EndsWith(".js") ||
                    path.EndsWith(".png") ||
                    path.EndsWith(".jpg") ||
                    path.EndsWith(".jpeg") ||
                    path.EndsWith(".gif") ||
                    path.EndsWith(".svg") ||
                    path.EndsWith(".ico") ||
                    path.EndsWith(".woff") ||
                    path.EndsWith(".woff2") ||
                    path.EndsWith(".ttf") ||
                    path.EndsWith(".eot") ||
                    path.EndsWith(".map") ||
                    path.EndsWith(".webp"))
                {
                    return;
                }

                // Allow UnderConstruction, Error, Admin area, and Admin login/2FA/logoff endpoints
                if (path.StartsWith("/underconstruction") ||
                    path.StartsWith("/error") ||
                    path.StartsWith("/admin") ||
                    path.StartsWith("/account/adminlogin") ||
                    path.StartsWith("/account/verifyauthenticator") ||
                    path.StartsWith("/account/logoff"))
                {
                    return;
                }

                ISettingService settingService = null;
                try
                {
                    settingService = DependencyResolver.Current?.GetService(typeof(ISettingService)) as ISettingService;
                }
                catch
                {
                    settingService = null;
                }

                if (settingService == null)
                {
                    return;
                }

                var isUnderConstruction = settingService.GetSettingByKey(Domain.Constants.IsSiteUnderConstruction).ToBool(false);
                if (!isUnderConstruction)
                {
                    return;
                }

                // Check IP whitelist if offline file exists
                var ipAddress = Context.Request.UserHostAddress;
                var offlineHelper = new Domain.Helpers.OfflineHelper(ipAddress, Server.MapPath);
                if (Domain.Helpers.OfflineHelper.OfflineData != null && !offlineHelper.ThisUserShouldBeOffline)
                {
                    return;
                }

                // Check if user is authenticated admin/editor
                var user = Context.User;
                if (user != null && user.Identity != null && user.Identity.IsAuthenticated &&
                    (user.IsInRole(Domain.Constants.AdministratorRole) || user.IsInRole(Domain.Constants.EditorRole)))
                {
                    return;
                }

                Response.Clear();
                Response.StatusCode = 302;
                Response.Status = "302 Found";
                Response.RedirectLocation = "/underconstruction";
                Response.End();
            }
            catch (ThreadAbortException)
            {
                // Normal ASP.NET behavior on Response.End()
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error in EnforceUnderConstructionRedirect");
            }
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            DependencyInjectionConfig.EndRequestScope();
        }

        protected void Application_End()
        {
            ObservabilityBootstrap.Shutdown();
            try
            {
                LogManager.Shutdown();
            }
            catch
            {
                // Suppress shutdown errors
            }
        }

        /// <summary>
        /// Enforces the canonical domain name by permanently redirecting requests for the "naked" domain 
        /// (e.g., test-site.com.tr) to the "www." prefix (e.g., www.test-site.com.tr).
        /// 
        /// Minimal Fix for Reverse Proxies/Tunnels:
        /// The previous logic relied on a heuristic (checking if the dot was past the midpoint of the host name) 
        /// to identify naked domains. This accidentally matched tunnel domains like `refill-juniper-amigo.ngrok-free.dev`, 
        /// causing broken redirects that corrupted the port and scheme.
        /// The updated logic requires an exact match on the configured production domain (excluding "www."), 
        /// ensuring development, staging, and tunnel requests are completely ignored while preserving production behavior.
        /// </summary>
        private void Redirect301()
        {
            var domain = AppConfig.Domain;

            // Ensure the configured domain expects 'www.', and that the incoming request is exactly the naked version of it.
            if (!string.IsNullOrEmpty(domain) && 
                domain.StartsWith("www.", StringComparison.OrdinalIgnoreCase) && 
                Request.Url.Host.Equals(domain.Substring(4), StringComparison.OrdinalIgnoreCase))
            {
                UriBuilder builder = new UriBuilder(Request.Url);
                builder.Host = "www." + Request.Url.Host;
                //builder.Host = Request.Url.Host;
                Response.StatusCode = 301;
                builder.Scheme = AppConfig.HttpProtocol;
                Response.AddHeader("Location", builder.ToString());
                Response.End();
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            redirectErrorController(sender);
        }

        private void redirectErrorController(object sender)
        {
            Exception exception = Server.GetLastError();
            var httpContext = ((MvcApplication)sender).Context;
            string currentController;
            string currentAction;
            TryGetRouteControllerAndAction(httpContext, out currentController, out currentAction);

            LogApplicationError(exception, httpContext, currentController, currentAction);

            if (!ShouldUseCustomErrorPage())
            {
                return;
            }

            if (IsAjaxRequest())
            {
                WriteAjaxErrorResponse(httpContext, exception);
            }
            else
            {
                ExecuteErrorController(httpContext, exception, currentController, currentAction);
            }
        }

        private static bool ShouldUseCustomErrorPage()
        {
            return HttpContext.Current == null || !HttpContext.Current.IsDebuggingEnabled;
        }

        private static void TryGetRouteControllerAndAction(HttpContext httpContext, out string currentController, out string currentAction)
        {
            currentController = " ";
            currentAction = " ";
            var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

            if (currentRouteData == null)
            {
                return;
            }

            if (currentRouteData.Values["controller"] != null &&
                !String.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
            {
                currentController = currentRouteData.Values["controller"].ToString();
            }

            if (currentRouteData.Values["action"] != null &&
                !String.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
            {
                currentAction = currentRouteData.Values["action"].ToString();
            }
        }

        private static void LogApplicationError(Exception exception, HttpContext httpContext, string currentController, string currentAction)
        {
            var requestUrl = httpContext.Request.Url.ToStr();
            String requestUrlReferrer = "";
            if (httpContext.Request.UrlReferrer != null)
            {
                requestUrlReferrer = httpContext.Request.UrlReferrer.ToStr();
            }
            var logMessage = (String.IsNullOrEmpty(requestUrlReferrer)
                                  ? ""
                                  : "requestUrlReferrer:" + requestUrlReferrer) + " requestUrl: " + requestUrl +
                             "  Controller:" +
                             currentController + " Action:" + currentAction + " error:" +
                             (exception != null ? exception.Message : "(null)");
            if (exception != null)
            {
                Logger.Error(exception, logMessage, "");
            }
            else
            {
                Logger.Error(logMessage);
            }
        }

        private static int GetErrorHttpStatusCode(Exception exception)
        {
            return exception is HttpException
                ? ((HttpException)exception).GetHttpCode()
                : 500;
        }

        private static void WriteAjaxErrorResponse(HttpContext httpContext, Exception exception)
        {
            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = GetErrorHttpStatusCode(exception);
            httpContext.Response.TrySkipIisCustomErrors = true;
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.Write("{\"success\":false,\"message\":\"An unexpected error occurred.\"}");
        }

        private static void ExecuteErrorController(HttpContext httpContext, Exception exception, string currentController, string currentAction)
        {
            var statusCode = GetErrorHttpStatusCode(exception);
            var action = "Index";

            switch (statusCode)
            {
                case 404:
                    action = "NotFound";
                    break;
                case 400:
                    action = "BadRequest";
                    break;
                case 401:
                    action = "Unauthorized";
                    break;
                case 403:
                    action = "Forbidden";
                    break;
                case 405:
                    action = "MethodNotAllowed";
                    break;
                case 500:
                default:
                    action = "InternalServerError";
                    break;
            }

            try
            {
                httpContext.ClearError();
                httpContext.Response.Clear();
                httpContext.Response.StatusCode = statusCode;
                httpContext.Response.TrySkipIisCustomErrors = true;

                var controller = new ErrorController();
                var routeData = new RouteData();
                routeData.Values["controller"] = "Error";
                routeData.Values["action"] = action;

                controller.ViewData.Model = new HandleErrorInfo(exception, currentController, currentAction);
                ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "ExecuteErrorController execution failed. Rendering static fallback error page.");
                try
                {
                    httpContext.ClearError();
                    httpContext.Response.Clear();
                    httpContext.Response.StatusCode = statusCode;
                    httpContext.Response.TrySkipIisCustomErrors = true;
                    httpContext.Response.ContentType = "text/html; charset=utf-8";

                    var title = statusCode == 404 ? "Sayfa Bulunamadı" : "Bir Hata Oluştu";
                    var message = statusCode == 404
                        ? "Aradığınız sayfa bulunamadı veya taşınmış olabilir."
                        : "Üzgünüz, işleminiz sırasında beklenmeyen bir hata meydana geldi.";

                    httpContext.Response.Write(
                        "<!DOCTYPE html><html lang=\"tr\"><head><meta charset=\"utf-8\" /><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" /><title>" +
                        title + "</title><style>body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;background:#f8fafc;color:#1e293b;margin:0;display:flex;align-items:center;justify-content:center;min-height:100vh;padding:20px;box-sizing:border-box}.card{background:#fff;border:1px solid #e2e8f0;border-radius:16px;padding:40px;max-width:540px;width:100%;text-align:center;box-shadow:0 10px 25px -5px rgba(0,0,0,0.05)}.code{font-size:56px;font-weight:800;color:#e11d48;line-height:1;margin-bottom:12px}h1{font-size:22px;font-weight:700;color:#0f172a;margin:0 0 12px}p{color:#64748b;font-size:15px;line-height:1.6;margin:0 0 24px}.btn{display:inline-block;padding:12px 28px;background:#067a36;color:#fff;text-decoration:none;border-radius:8px;font-weight:600;font-size:15px}</style></head><body><div class=\"card\"><div class=\"code\">" +
                        statusCode + "</div><h1>" + title + "</h1><p>" + message + "</p><a href=\"/\" class=\"btn\">Ana Sayfaya Dön</a></div></body></html>");
                }
                catch
                {
                    // Ignore secondary output failures
                }
            }
        }

        // Checks whether the request is an AJAX or JSON-expecting request without expensive runtime reflection
        private bool IsAjaxRequest()
        {
            if (Request == null)
            {
                return false;
            }

            // 1. Standard ASP.NET MVC AJAX check (X-Requested-With: XMLHttpRequest)
            if (new HttpRequestWrapper(Request).IsAjaxRequest())
            {
                return true;
            }

            // 2. Direct Header Check
            if (string.Equals(Request.Headers?["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 3. JSON Content-Type
            if (!string.IsNullOrEmpty(Request.ContentType) &&
                Request.ContentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            // 4. JSON Accept Header
            var acceptTypes = Request.AcceptTypes;
            if (acceptTypes != null)
            {
                for (int i = 0; i < acceptTypes.Length; i++)
                {
                    if (acceptTypes[i] != null &&
                        acceptTypes[i].IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}