using EImece.Controllers;
using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Scheduler;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Http;
using System.Web.Routing;

namespace EImece
{
    public class MvcApplication : System.Web.HttpApplication
    {

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            MvcHandler.DisableMvcResponseHeader = true;
            var quartzService = DependencyResolver.Current.GetService<QuartzService>();
            quartzService.StartSchedulerService();

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        public override string GetVaryByCustomString(HttpContext context, string arg)
        {
            if (arg == "User")
            {
                if (context.Request.IsAuthenticated)
                {
                    return string.Format("User:{0}-Rnd:{1}", context.User.Identity.Name, Guid.NewGuid().ToString());
                }
                else
                {
                    return ""; // context.User.Identity.Name;
                }

            }

            return base.GetVaryByCustomString(context, arg);

        }

       
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            Redirect301();
        }
        private void Redirect301()
        {
            var domain = Settings.Domain;


            if (domain.StartsWith("www") && !Request.Url.Host.StartsWith("www") && !Request.Url.IsLoopback
                && Request.Url.Host.IndexOf('.') > Request.Url.Host.Length / 2
                )
            {
                UriBuilder builder = new UriBuilder(Request.Url);
                // builder.Host = "www." + Request.Url.Host;
                builder.Host =  Request.Url.Host;
                Response.StatusCode = 301;
                builder.Scheme = Settings.HttpProtocol;
                Response.AddHeader("Location", builder.ToString());
                Response.End();
            }
        }
        protected void Application_Error(object sender, EventArgs e)
        {

            bool useCustomError = true;

            String siteStatus = Settings.GetConfigString("SiteStatus", "dev");

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
