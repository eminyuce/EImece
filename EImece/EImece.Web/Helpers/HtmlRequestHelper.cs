using System;
using System.Linq;
using System.Web;

namespace EImece.Web.Helpers
{
    public class HtmlRequestHelper
    {
        public static string Id()
        {
            var routeValues = HttpContext.Current?.Request?.RequestContext?.RouteData?.Values;

            if (routeValues != null && routeValues.ContainsKey("id"))
                return (string)routeValues["id"];
            else if (HttpContext.Current?.Request?.QueryString?.AllKeys?.Contains("id") == true)
                return HttpContext.Current.Request.QueryString["id"];

            return string.Empty;
        }

        public static string Controller()
        {
            var routeValues = HttpContext.Current?.Request?.RequestContext?.RouteData?.Values;

            if (routeValues != null && routeValues.ContainsKey("controller"))
                return (string)routeValues["controller"];

            return string.Empty;
        }

        public static string Action()
        {
            var routeValues = HttpContext.Current?.Request?.RequestContext?.RouteData?.Values;

            if (routeValues != null && routeValues.ContainsKey("action"))
                return (string)routeValues["action"];

            return string.Empty;
        }
    }
}
