using EImece.Domain.Services.IServices;
using System;
using System.Web.Mvc;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Reads dynamic Admin & Storefront settings from ISettingService with constant fallbacks.
    /// </summary>
    public static class AdminSettings
    {
        public static int GridPageSizeNumber
        {
            get
            {
                var settingService = DependencyResolver.Current?.GetService(typeof(ISettingService)) as ISettingService;
                var val = settingService?.GetSettingByKey(Constants.GridPageSizeNumber);
                return !string.IsNullOrWhiteSpace(val) ? val.ToInt(Constants.DefaultGridPageSizeNumber) : Constants.DefaultGridPageSizeNumber;
            }
        }

        public static int ProductShortDescriptionPreviewLength
        {
            get
            {
                var settingService = DependencyResolver.Current?.GetService(typeof(ISettingService)) as ISettingService;
                var val = settingService?.GetSettingByKey(Constants.ProductShortDescriptionPreviewLength);
                return !string.IsNullOrWhiteSpace(val) ? val.ToInt(Constants.DefaultProductShortDescriptionPreviewLength) : Constants.DefaultProductShortDescriptionPreviewLength;
            }
        }
    }
}
