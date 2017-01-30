using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;

namespace EImece.Domain.Services
{
    public class SettingService : BaseEntityService<Setting>, ISettingService
    {
        private ISettingRepository SettingRepository { get; set; }
        public SettingService(ISettingRepository repository) : base(repository)
        {
            SettingRepository = repository;
        }
        private List<Setting> GetAllSettings()
        {
            var cacheKey = String.Format("Settings");
            List<Setting> result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = SettingRepository.GetAllSettings();
                MemoryCacheProvider.Set(cacheKey, result, Settings.CacheLongSeconds);
            }
            return result;
        }
        public string GetSettingByKey(string key)
        {
            var allSettings = GetAllSettings();
            var result = allSettings.FirstOrDefault(r => r.SettingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase));
            if (result != null)
            {
                return result.SettingValue;
            }
            else
            {
                return String.Empty;
            }

        }
    }
}
