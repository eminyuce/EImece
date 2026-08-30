using System.Web.Mvc;
using EImece.Web.Infrastructure.Designs;

namespace EImece.App_Start
{
    public static class ViewEngineConfig
    {
        public static void RegisterViewEngines(ViewEngineCollection engines)
        {
            if (engines == null) return;
            engines.Clear();
            engines.Add(new DesignAwareRazorViewEngine());
        }
    }
}
