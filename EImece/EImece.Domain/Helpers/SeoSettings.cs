using EImece.Domain.Services.IServices;
using System.Web.Mvc;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Reads the AllowSearchEngineIndexing setting dynamically from ISettingService (database)
    /// with default constant fallback false.
    /// </summary>
    public static class SeoSettings
    {
        public static bool AllowIndexing
        {
            get
            {
                var settingService = DependencyResolver.Current?.GetService(typeof(ISettingService)) as ISettingService;
                return settingService?.GetSettingByKey(Constants.AllowSearchEngineIndexing).ToBool(Constants.DefaultAllowSearchEngineIndexing)
                       ?? Constants.DefaultAllowSearchEngineIndexing;
            }
        }
    }
}
