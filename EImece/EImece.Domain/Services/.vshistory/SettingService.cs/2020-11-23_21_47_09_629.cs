using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EImece.Domain.Services
{
    public class SettingService : BaseEntityService<Setting>, ISettingService
    {
        private ISettingRepository SettingRepository { get; set; }

        public SettingService(ISettingRepository repository) : base(repository)
        {
            SettingRepository = repository;
        }

        public List<Setting> GetAllActiveSettings()
        {
            var cacheKey = String.Format("GetAllActiveSettings");
            List<Setting> result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = SettingRepository.GetAllActiveSettings();
                MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheLongSeconds);
            }
            return result;
        }

        private List<Setting> GetAllSettings()
        {
            var cacheKey = "GetAllSettings" ;
            List<Setting> result = null;

            if (!MemoryCacheProvider.Get(cacheKey, out result))
            {
                result = SettingRepository.GetAllSettings();
                MemoryCacheProvider.Set(cacheKey, result, AppConfig.CacheLongSeconds);
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
        public string GetSettingByKey(string key, int language)
        {
            var allSettings = GetAllSettings();
            var result = allSettings.FirstOrDefault(r => r.Lang == language && r.SettingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase));
            if (result != null)
            {
                return result.SettingValue;
            }
            else
            {
                return String.Empty;
            }
        }

        public Setting GetSettingObjectByKey(string key)
        {
            var allSettings = GetAllSettings();
            var result = allSettings.FirstOrDefault(r => r.SettingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase));
            if (result != null)
            {
                return result;
            }
            else
            {
                var setting = EntityFactory.GetBaseEntityInstance<Setting>();
                setting.SettingKey = key;
                setting.SettingValue = key;
                return setting;
            }
        }

        public Setting GetSettingObjectByKey(string key, int language)
        {
            var allSettings = GetAllSettings();
            var result = allSettings.FirstOrDefault(r => r.Lang == language && r.SettingKey.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (result != null)
            {
                return result;
            }
            else
            {
                var setting = EntityFactory.GetBaseEntityInstance<Setting>();
                setting.SettingKey = key;
                setting.SettingValue = key;
                setting.Lang = language;
                return setting;
            }
        }
        public SystemSettingModel GetSystemSettingModel()
        {
            var result = new SystemSettingModel();

            Type type = result.GetType();
            List<Setting> Settings = GetAllSettings().Where(r => Constants.SystemSettings.Equals(r.Description,StringComparison.InvariantCultureIgnoreCase)).ToList();
            // Loop over properties.
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                // Get name.
                string name = propertyInfo.Name;
                // Get value on the target instance.
                var setting = Settings.FirstOrDefault(r => r.SettingKey.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (setting != null)
                {
                    if (propertyInfo.PropertyType == typeof(int))
                    {
                        propertyInfo.SetValue(result, setting.SettingValue.ToInt(), null);
                    }
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        propertyInfo.SetValue(result, setting.SettingValue.ToStr(), null);
                    }
                    if (propertyInfo.PropertyType == typeof(bool))
                    {
                        propertyInfo.SetValue(result, setting.SettingValue.ToBool(), null);
                    }
                }
            }

            return result;
        }

        public void SaveSystemSettingModel(SystemSettingModel settingModel)
        {
            List<Setting> Settings = GetAllSettings();
            // Get type.
            Type type = settingModel.GetType();

            // Loop over properties.
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                // Get name.
                string name = propertyInfo.Name;

                // Get value on the target instance.
                object value = propertyInfo.GetValue(settingModel, null);
                var setting = Settings.FirstOrDefault(r =>   r.SettingKey.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (setting == null)
                {
                    var newSetting = new Setting();
                    newSetting.Name = name;
                    newSetting.IsActive = true;
                    newSetting.SettingKey = name;
                    newSetting.Description = Constants.SystemSettings;
                    newSetting.SettingValue = value.ToStr();
                    SaveOrEditEntity(newSetting);
                }
                else
                {
                    setting.Description = Constants.SystemSettings;
                    setting.SettingValue = value.ToStr();
                    SaveOrEditEntity(setting);
                }
            }
        }
        public SettingModel GetSettingModel(int language)
        {
            var result = new SettingModel();

            Type type = result.GetType();
            List<Setting> Settings = GetAllSettings().Where(r => r.Lang == language && Constants.AdminSetting.Equals(r.Description, StringComparison.InvariantCultureIgnoreCase)).ToList();
            // Loop over properties.
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                // Get name.
                string name = propertyInfo.Name;

                // Get value on the target instance.

                var setting = Settings.FirstOrDefault(r => r.Lang == language && r.SettingKey.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (setting != null)
                {
                    if (propertyInfo.PropertyType == typeof(int))
                    {
                        propertyInfo.SetValue(result, setting.SettingValue.ToInt(), null);
                    }
                    if (propertyInfo.PropertyType == typeof(string))
                    {
                        propertyInfo.SetValue(result, setting.SettingValue.ToStr(), null);
                    }
                    if (propertyInfo.PropertyType == typeof(bool))
                    {
                        propertyInfo.SetValue(result, setting.SettingValue.ToBool(), null);
                    }
                }
            }

            return result;
        }

        public void SaveSettingModel(SettingModel settingModel, int  lang)
        {
            List<Setting> Settings = GetAllSettings();
            // Get type.
            Type type = settingModel.GetType();

            // Loop over properties.
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                // Get name.
                string name = propertyInfo.Name;

                // Get value on the target instance.
                object value = propertyInfo.GetValue(settingModel, null);
                var setting = Settings.FirstOrDefault(r => r.Lang == lang && r.SettingKey.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (setting == null)
                {
                    var newSetting = new Setting();
                    newSetting.Name = name;
                    newSetting.IsActive = true;
                    newSetting.SettingKey = name;
                    newSetting.Description = Constants.AdminSetting;
                    newSetting.SettingValue = value.ToStr();
                    newSetting.Lang = lang;
                    SaveOrEditEntity(newSetting);
                }
                else
                {
                    setting.Description = Constants.AdminSetting;
                    setting.SettingValue = value.ToStr();
                    setting.Lang = lang;
                    SaveOrEditEntity(setting);
                }
            }
        }

        public EmailAccount GetEmailAccount()
        {
            var emailAccount = new EmailAccount();
            emailAccount.Host = GetSettingByKey(Constants.AdminEmailHost);
            emailAccount.Password = GetSettingByKey(Constants.AdminEmailPassword);
            emailAccount.EnableSsl = GetSettingByKey(Constants.AdminEmailEnableSsl).ToBool();
            emailAccount.Port = GetSettingByKey(Constants.AdminEmailPort).ToInt();
            emailAccount.UseDefaultCredentials = GetSettingByKey(Constants.AdminEmailUseDefaultCredentials).ToBool();
            emailAccount.Email = GetSettingByKey(Constants.AdminEmail);
            emailAccount.Username = GetSettingByKey(Constants.AdminUserName).ToStr();
            emailAccount.Email = String.IsNullOrEmpty(emailAccount.Email) ? emailAccount.Username : emailAccount.Email;

            emailAccount.DisplayName = GetSettingByKey(Constants.AdminEmailDisplayName);
            emailAccount.DisplayName = String.IsNullOrEmpty(emailAccount.DisplayName) ? emailAccount.Username : emailAccount.DisplayName;
         
            return emailAccount;
        }

      
    }
}