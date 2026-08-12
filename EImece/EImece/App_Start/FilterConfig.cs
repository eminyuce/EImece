using System.Web.Mvc;

namespace EImece
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new Filters.RequestLoggingActionFilter());
            filters.Add(new Filters.RobotsNoIndexAttribute());
        }
    }
}