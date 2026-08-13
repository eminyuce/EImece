using EImece.App_Start;
using EImece.Controllers;
using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Services;
using Microsoft.AspNet.Identity;
using NLog;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Helpers;
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

            // Required for AntiForgeryToken with claims-based auth (including BypassAdminAuth debug principal).
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
        /// Invokes canonical URL enforcement.
        /// </summary>
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            DependencyInjectionConfig.BeginRequestScope();
            Redirect301();
        }

        /// <summary>
        /// TEMPORARY: when BypassAdminAuth is enabled (non-live + local only), inject a debug Admin principal
        /// so the admin sidebar/layout and role-gated menus can be smoke-tested without AdminLogin.
        /// Only applies to /admin requests so storefront logout/home stay anonymous.
        /// </summary>
        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            if (!AppConfig.BypassAdminAuth)
            {
                return;
            }

            // Never inject a privileged principal for non-local requests.
            if (Context == null || !Context.Request.IsLocal)
            {
                return;
            }

            var path = (Context.Request.AppRelativeCurrentExecutionFilePath ?? Context.Request.Path ?? "")
                .TrimStart('~')
                .ToLowerInvariant();
            if (!path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
            {
                return;
            }

            var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
            identity.AddClaim(new Claim(ClaimTypes.Name, "debug-admin"));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "debug-admin"));
            identity.AddClaim(new Claim(
                "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider",
                "LocalDebug"));
            identity.AddClaim(new Claim(ClaimTypes.Role, Domain.Constants.AdministratorRole));
            var principal = new ClaimsPrincipal(identity);
            Context.User = principal;
            Thread.CurrentPrincipal = principal;
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            DependencyInjectionConfig.EndRequestScope();
        }

        protected void Application_End()
        {
            ObservabilityBootstrap.Shutdown();
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
            // When detailed errors are enabled (appSetting or compilation debug), leave the exception
            // for ASP.NET's yellow screen / MVC detailed error output.
            return !AppConfig.ExposeDetailedErrors
                && (HttpContext.Current == null || !HttpContext.Current.IsDebuggingEnabled);
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
            var controller = new ErrorController();
            var routeData = new RouteData();
            var action = "Index";

            httpContext.ClearError();
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = GetErrorHttpStatusCode(exception);
            httpContext.Response.TrySkipIisCustomErrors = true;

            routeData.Values["controller"] = "Error";
            routeData.Values["action"] = action;

            controller.ViewData.Model = new HandleErrorInfo(exception, currentController, currentAction);
            ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
        }

        //This method checks if we have an AJAX request or not
        private bool IsAjaxRequest()
        {
            //The easy way
            bool isAjaxRequest = (Request["X-Requested-With"] == "XMLHttpRequest")
            || ((Request.Headers != null)
            && (Request.Headers["X-Requested-With"] == "XMLHttpRequest"));

            //If we are not sure that we have an AJAX request or that we have to return JSON
            //we fall back to Reflection
            if (!isAjaxRequest)
            {
                try
                {
                    //The controller and action
                    string controllerName = Request.RequestContext.
                                            RouteData.Values["controller"].ToString();
                    string actionName = Request.RequestContext.
                                        RouteData.Values["action"].ToString();

                    //We create a controller instance
                    DefaultControllerFactory controllerFactory = new DefaultControllerFactory();
                    Controller controller = controllerFactory.CreateController(
                    Request.RequestContext, controllerName) as Controller;

                    //We get the controller actions
                    ReflectedControllerDescriptor controllerDescriptor =
                    new ReflectedControllerDescriptor(controller.GetType());
                    ActionDescriptor[] controllerActions =
                    controllerDescriptor.GetCanonicalActions();

                    //We search for our action
                    foreach (ReflectedActionDescriptor actionDescriptor in controllerActions)
                    {
                        if (actionDescriptor.ActionName.ToUpper().Equals(actionName.ToUpper()))
                        {
                            //If the action returns JsonResult then we have an AJAX request
                            if (actionDescriptor.MethodInfo.ReturnType == typeof(JsonResult))
                                return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "IsAjaxRequest reflection check failed");
                }
            }

            return isAjaxRequest;
        }
    }
}