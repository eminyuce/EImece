using EImece.Domain;
using EImece.Domain.Services.IServices;
using System.Web.Mvc;

namespace EImece.Web.Infrastructure.Designs
{
    public class ConfigDesignProvider : IDesignProvider
    {
        public string GetActiveDesign()
        {
            var settingService = DependencyResolver.Current?.GetService<ISettingService>();
            var design = settingService?.GetSettingByKey(Constants.ActiveDesign);
            if (!string.IsNullOrWhiteSpace(design))
            {
                return design.Trim();
            }

            return Constants.DefaultActiveDesign;
        }
    }
}
