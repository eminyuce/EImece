using EImece.Web.Filters;
using System.Web.Mvc;

namespace EImece
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new RequestLoggingActionFilter());
            filters.Add(new RobotsNoIndexAttribute());
            filters.Add(new UnderConstAttribute());
        }
    }
}