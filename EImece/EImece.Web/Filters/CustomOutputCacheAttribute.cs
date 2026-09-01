using EImece.Domain;
using EImece.Domain.DependencyInjection;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using EImece.Web.Caching;
using System.Web.Mvc;

namespace EImece.Web.Filters
{
    public class CustomOutputCacheAttribute : OutputCacheAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            var isUnderConstruction = IsSiteUnderConstruction();

            if (isUnderConstruction || httpContext.User.Identity.IsAuthenticated)
            {
                try { OutputCacheRequestProbe.MarkBypassed(httpContext); } catch { }
                // Skip output-cache when under construction or for authenticated users
                httpContext.Response.Cache.SetNoServerCaching();
                httpContext.Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
                httpContext.Response.Cache.SetNoStore();
            }
            else
            {
                if (IsHtmlStorefrontProfile())
                {
                    try { OutputCacheRequestProbe.MarkPageGeneration(httpContext); } catch { }
                }
                base.OnActionExecuting(filterContext);
            }
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            var isUnderConstruction = IsSiteUnderConstruction();

            if (isUnderConstruction || httpContext.User.Identity.IsAuthenticated)
            {
                httpContext.Response.Cache.SetNoServerCaching();
                httpContext.Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
                httpContext.Response.Cache.SetNoStore();
            }
            else
            {
                base.OnResultExecuting(filterContext);
            }
        }

        private static bool IsSiteUnderConstruction()
        {
            try
            {
                var settingService = DomainServiceProvider.GetService<ISettingService>();
                return settingService != null && settingService.GetSettingByKey(Constants.IsSiteUnderConstruction).ToBool(false);
            }
            catch
            {
                return false;
            }
        }

        private bool IsHtmlStorefrontProfile()
        {
            var profile = CacheProfile;
            return string.Equals(profile, Constants.Cache1Hour, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(profile, Constants.Cache20Minutes, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(profile, Constants.Cache1Day, System.StringComparison.OrdinalIgnoreCase);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            // Never cache error responses (avoids blank 500 pages being served from output cache).
            if (filterContext != null
                && filterContext.HttpContext != null
                && filterContext.HttpContext.Response.StatusCode >= 400)
            {
                filterContext.HttpContext.Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
                filterContext.HttpContext.Response.Cache.SetNoStore();
                filterContext.HttpContext.Response.Cache.SetNoServerCaching();
            }

            base.OnResultExecuted(filterContext);
        }
    }
}
