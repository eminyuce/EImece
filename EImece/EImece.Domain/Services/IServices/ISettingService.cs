using EImece.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EImece.Domain.Models.AdminModels;

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
