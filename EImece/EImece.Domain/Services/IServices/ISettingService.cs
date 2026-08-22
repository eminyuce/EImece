using EImece.Domain.Entities;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.AdminModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services.IServices
{
    public interface ISettingService : IBaseEntityService<Setting>
    {
        string GetSettingByKey(string key);

        string GetSettingByKey(string key, int language);

        Task<string> GetSettingByKeyAsync(string key);

        Task<string> GetSettingByKeyAsync(string key, int language);

        Setting GetSettingObjectByKey(string key);

        Setting GetSettingObjectByKey(string key, int language);

        Task<Setting> GetSettingObjectByKeyAsync(string key);

        Task<Setting> GetSettingObjectByKeyAsync(string key, int language);

        Models.DTOs.SettingDto GetSettingDtoByKey(string key);

        Models.DTOs.SettingDto GetSettingDtoByKey(string key, int language);

        Task<Models.DTOs.SettingDto> GetSettingDtoByKeyAsync(string key);

        Task<Models.DTOs.SettingDto> GetSettingDtoByKeyAsync(string key, int language);

        // Minimal projection — single column, no cache amplification
        string GetSettingValue(string key);
        string GetSettingValue(string key, int language);
        Task<string> GetSettingValueAsync(string key, CancellationToken cancellationToken = default(CancellationToken));
        Task<string> GetSettingValueAsync(string key, int language, CancellationToken cancellationToken = default(CancellationToken));
        Models.DTOs.Storefront.SettingValueDto GetSettingValueDtoByKey(string key);
        Models.DTOs.Storefront.SettingValueDto GetSettingValueDtoByKey(string key, int language);
        Task<Models.DTOs.Storefront.SettingValueDto> GetSettingValueDtoByKeyAsync(string key, CancellationToken cancellationToken = default(CancellationToken));
        Task<Models.DTOs.Storefront.SettingValueDto> GetSettingValueDtoByKeyAsync(string key, int language, CancellationToken cancellationToken = default(CancellationToken));
        Dictionary<string, string> GetSettingValues(IEnumerable<string> keys);
        Task<Dictionary<string, string>> GetSettingValuesAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default(CancellationToken));
        List<Models.DTOs.Storefront.SettingKeyValueDto> GetSettingKeyValues(int language);
        Task<List<Models.DTOs.Storefront.SettingKeyValueDto>> GetSettingKeyValuesAsync(int language, CancellationToken cancellationToken = default(CancellationToken));

        SettingModel GetSettingModel(int language);

        Task<SettingModel> GetSettingModelAsync(int language);

        void SaveSettingModel(SettingModel settingModel, int lang);

        Task SaveSettingModelAsync(SettingModel settingModel, int lang);

        List<Setting> GetAllActiveSettings();

        Task<List<Setting>> GetAllActiveSettingsAsync();

        EmailAccount GetEmailAccount();

        Task<EmailAccount> GetEmailAccountAsync();

        SystemSettingModel GetSystemSettingModel();

        Task<SystemSettingModel> GetSystemSettingModelAsync(CancellationToken cancellationToken = default(CancellationToken));

        void SaveSystemSettingModel(SystemSettingModel settingModel);

        Task SaveSystemSettingModelAsync(SystemSettingModel settingModel);

        Dictionary<string, string> CreateShareableSocialMediaLinks(string link, string text, string imagefullPath);

        void ClearCache();
    }
}