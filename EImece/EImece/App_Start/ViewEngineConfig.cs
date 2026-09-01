using EImece.Web.Infrastructure.Designs;
using System.Web.Mvc;

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
