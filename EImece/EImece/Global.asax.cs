using EImece.App_Start;
using EImece.Controllers;
using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Services;
using NLog;
using System;
using System.Net;
using System.Web;
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
            //System.Net.ServicePointManager.SecurityProtocol
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // TLS 1.2 only; older protocols (TLS 1.0/1.1) removed for security

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            MvcHandler.DisableMvcResponseHeader = true;

            ObservabilityBootstrap.Configure();
            GlobalFilters.Filters.Add(new Filters.MetricsActionFilter(
                DependencyResolver.Current.GetService<Domain.Observability.Metrics.IApplicationMetrics>()));
            GlobalFilters.Filters.Add(new Filters.StructuredExceptionFilter());

            var adresService = DependencyResolver.Current.GetService<AdresService>();
            //  var quartzService = DependencyResolver.Current.GetService<QuartzService>();
            //  quartzService.StartSchedulerService();

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            if (custom == "User")
            {
                HttpCookie cultureCookie = Request.Cookies[Constants.CultureCookieName];
                String cultureCookieValue = "";
                if (cultureCookie != null)
                {
                    cultureCookieValue = cultureCookie.Values[Constants.ELanguage].ToStr();
                }

                if (User.Identity.IsAuthenticated)
                {
                    return string.Format("User:{0}-Rnd:{1}:Lang:{2}",
                        context.User.Identity.Name,
                        Guid.NewGuid().ToString(),
                        cultureCookieValue);
                }
                else
                {
                    return string.Format("cultureCookieValue:{0}-Rnd:{1}",
                    cultureCookieValue,
                    Guid.NewGuid().ToString());
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
            Redirect301();
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
            // redirectErrorController(sender);
        }

        private void redirectErrorController(object sender)
        {
            bool useCustomError = true;

            String siteStatus = AppConfig.GetConfigString("SiteStatus", "dev");

            if (siteStatus.IndexOf("live", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                useCustomError = true;
            }
            else
            {
                useCustomError = false;
            }

            if (useCustomError)
            {
                var httpContext = ((MvcApplication)sender).Context;
                var currentController = " ";
                var currentAction = " ";
                var currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

                if (currentRouteData != null)
                {
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

                Exception exception = Server.GetLastError();

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
                                 currentController + " Action:" + currentAction + " error:" + exception.Message;
                Logger.Error(exception, logMessage, "");
                //We check if we have an AJAX request and return JSON in this case
                if (IsAjaxRequest())
                {
                }
                else
                {
                    var controller = new ErrorController();
                    var routeData = new RouteData();
                    var action = "Index";

                    httpContext.ClearError();
                    httpContext.Response.Clear();
                    httpContext.Response.StatusCode = exception is HttpException
                                                          ? ((HttpException)exception).GetHttpCode()
                                                          : 500;
                    httpContext.Response.TrySkipIisCustomErrors = true;

                    routeData.Values["controller"] = "Error";
                    routeData.Values["action"] = action;

                    controller.ViewData.Model = new HandleErrorInfo(exception, currentController, currentAction);
                    ((IController)controller).Execute(new RequestContext(new HttpContextWrapper(httpContext), routeData));
                }
            }
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
                catch
                {
                }
            }

            return isAjaxRequest;
        }
    }
}