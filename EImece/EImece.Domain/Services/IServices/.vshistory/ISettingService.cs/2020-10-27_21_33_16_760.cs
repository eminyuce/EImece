using EImece.Domain.Entities;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.AdminModels;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface ISettingService : IBaseEntityService<Setting>
    {
        string GetSettingByKey(string key);
        string GetSettingByKey(string key, int language);
        Setting GetSettingObjectByKey(string key);

        Setting GetSettingObjectByKey(string key, int language);

        SettingModel GetSettingModel(int language);

        void SaveSettingModel(SettingModel settingModel, int  lang);

        List<Setting> GetAllActiveSettings();

        EmailAccount GetEmailAccount();
        SystemSettingModel GetSystemSettingModel();
        void SaveSystemSettingModel(SystemSettingModel settingModel);
    }
}