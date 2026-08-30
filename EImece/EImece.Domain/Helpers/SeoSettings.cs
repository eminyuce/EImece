using EImece.Domain.DependencyInjection;
using EImece.Domain.Services.IServices;

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
                var settingService = DomainServiceProvider.GetService<ISettingService>();
                return settingService?.GetSettingByKey(Constants.AllowSearchEngineIndexing).ToBool(Constants.DefaultAllowSearchEngineIndexing)
                       ?? Constants.DefaultAllowSearchEngineIndexing;
            }
        }
    }
}
