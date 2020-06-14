using EImece.Domain.Entities;
using EImece.Domain.Models.AdminModels;
using System.Collections.Generic;

namespace EImece.Domain.Services.IServices
{
    public interface ISettingService : IBaseEntityService<Setting>
    {
        string GetSettingByKey(string key);

        Setting GetSettingObjectByKey(string key);

        Setting GetSettingObjectByKey(string key, int language);

        SettingModel GetSettingModel();

        void SaveSettingModel(SettingModel settingModel);

        List<Setting> GetAllActiveSettings();
    }
}