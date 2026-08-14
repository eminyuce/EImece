using EImece.Domain.Caching;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using NLog;
using System;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class WebAppManifestService : IWebAppManifestService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ISettingService _settingService;
        private readonly IEimeceCacheProvider _cache;

        public WebAppManifestService(ISettingService settingService, IEimeceCacheProvider cache)
        {
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public Task<string> GetManifestJsonAsync()
        {
            return _cache.GetOrAddAsync(
                CacheKeys.WebAppManifest,
                BuildManifestJsonAsync,
                AppConfig.CacheLongSeconds);
        }

        private async Task<string> BuildManifestJsonAsync()
        {
            string companyName = null;
            string metaTitle = null;
            string metaDescription = null;
            string themeColorSetting = null;

            try
            {
                companyName = await _settingService.GetSettingByKeyAsync(Constants.CompanyName).ConfigureAwait(false);
                metaTitle = await _settingService.GetSettingByKeyAsync(Constants.SiteIndexMetaTitle).ConfigureAwait(false);
                metaDescription = await _settingService.GetSettingByKeyAsync(Constants.SiteIndexMetaDescription).ConfigureAwait(false);
                themeColorSetting = await _settingService.GetSettingByKeyAsync(Constants.ThemeColor).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to load web app manifest branding from settings; using fallbacks.");
            }

            return WebAppManifestHelper.BuildJson(
                companyName,
                metaTitle,
                metaDescription,
                themeColorSetting,
                AppConfig.ThemeColor,
                AppConfig.Domain);
        }
    }
}
