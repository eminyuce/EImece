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