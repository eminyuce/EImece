using EImece.Domain.Caching;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Domain.Services
{
    public class SettingService : BaseEntityService<Setting>, ISettingService
    {
        private ISettingRepository SettingRepository { get; set; }

        public const string NULL_VALUE = "RETURN-NULL-VALUE";
        private static string ALL_SETTING_CACHE_KEY = "ALL_SETTING_CACHE_KEY_V3";

        public SettingService(ISettingRepository repository) : base(repository)
        {
            SettingRepository = repository;
        }

        public List<Setting> GetAllActiveSettings()
        {
            return GetAllSettings().Where(t => t.IsActive).ToList();
        }

        public async Task<List<Setting>> GetAllActiveSettingsAsync()
        {
            var allSettings = await GetAllSettingsAsync().ConfigureAwait(false);
            return allSettings.Where(t => t.IsActive).ToList();
        }

        public void ClearCache()
        {
            if (DataCachingProvider != null)
            {
                DataCachingProvider.Clear(ALL_SETTING_CACHE_KEY);
                DataCachingProvider.Clear(ALL_SETTING_CACHE_KEY + AsyncCacheKeySuffix);
                DataCachingProvider.Clear(CacheKeys.WebAppManifest);
            }
        }

        private List<Setting> GetAllSettings()
        {
            if (DataCachingProvider == null)
            {
                return SettingRepository.GetAllSettings();
            }
            return DataCachingProvider.GetOrAdd(
                ALL_SETTING_CACHE_KEY,
                () => SettingRepository.GetAllSettings(),
                AppConfig.CacheLongSeconds);
        }

        private async Task<List<Setting>> GetAllSettingsAsync()
        {
            if (DataCachingProvider == null)
            {
                return await SettingRepository.GetAllSettingsAsync(CancellationToken.None).ConfigureAwait(false);
            }
            // CancellationToken.None: the cached task is shared by all concurrent misses.
            return await DataCachingProvider.GetOrAddAsync(
                ALL_SETTING_CACHE_KEY + AsyncCacheKeySuffix,
                () => SettingRepository.GetAllSettingsAsync(CancellationToken.None),
                AppConfig.CacheLongSeconds).ConfigureAwait(false);
        }

        public string GetSettingByKey(string key)
        {
            var result = GetSettingObjectByKey(key);
            if (result.SettingValue != NULL_VALUE && !string.IsNullOrWhiteSpace(result.SettingValue))
            {
                return result.SettingValue;
            }
            return System.Configuration.ConfigurationManager.AppSettings[key] ?? string.Empty;
        }

        public string GetSettingByKey(string key, int language)
        {
            var result = GetSettingObjectByKey(key, language);
            if (result.SettingValue != NULL_VALUE && !string.IsNullOrWhiteSpace(result.SettingValue))
            {
                return result.SettingValue;
            }
            return System.Configuration.ConfigurationManager.AppSettings[key] ?? string.Empty;
        }

        public async Task<string> GetSettingByKeyAsync(string key)
        {
            var result = await GetSettingObjectByKeyAsync(key).ConfigureAwait(false);
            if (result.SettingValue != NULL_VALUE && !string.IsNullOrWhiteSpace(result.SettingValue))
            {
                return result.SettingValue;
            }
            return System.Configuration.ConfigurationManager.AppSettings[key] ?? string.Empty;
        }

        public async Task<string> GetSettingByKeyAsync(string key, int language)
        {
            var result = await GetSettingObjectByKeyAsync(key, language).ConfigureAwait(false);
            if (result.SettingValue != NULL_VALUE && !string.IsNullOrWhiteSpace(result.SettingValue))
            {
                return result.SettingValue;
            }
            return System.Configuration.ConfigurationManager.AppSettings[key] ?? string.Empty;
        }

        public Setting GetSettingObjectByKey(string key)
        {
            return SelectSettingByKey(GetAllSettings(), key);
        }

        public Setting GetSettingObjectByKey(string key, int language)
        {
            return SelectSettingByKey(GetAllSettings(), key, language);
        }

        public async Task<Setting> GetSettingObjectByKeyAsync(string key)
        {
            var allSettings = await GetAllSettingsAsync().ConfigureAwait(false);
            return SelectSettingByKey(allSettings, key);
        }

        public async Task<Setting> GetSettingObjectByKeyAsync(string key, int language)
        {
            var allSettings = await GetAllSettingsAsync().ConfigureAwait(false);
            return SelectSettingByKey(allSettings, key, language);
        }

        public Models.DTOs.SettingDto GetSettingDtoByKey(string key)
        {
            var s = GetSettingObjectByKey(key);
            return MapToSettingDto(s);
        }

        public Models.DTOs.SettingDto GetSettingDtoByKey(string key, int language)
        {
            var s = GetSettingObjectByKey(key, language);
            return MapToSettingDto(s);
        }

        public async Task<Models.DTOs.SettingDto> GetSettingDtoByKeyAsync(string key)
        {
            var s = await GetSettingObjectByKeyAsync(key).ConfigureAwait(false);
            return MapToSettingDto(s);
        }

        public async Task<Models.DTOs.SettingDto> GetSettingDtoByKeyAsync(string key, int language)
        {
            var s = await GetSettingObjectByKeyAsync(key, language).ConfigureAwait(false);
            return MapToSettingDto(s);
        }

        private static Models.DTOs.SettingDto MapToSettingDto(Setting s)
        {
            if (s == null) return null;
            return new Models.DTOs.SettingDto
            {
                Id = s.Id,
                Name = s.Name,
                SettingKey = s.SettingKey,
                SettingValue = s.SettingValue,
                Lang = s.Lang,
                Position = s.Position,
                IsActive = s.IsActive,
                CreatedDate = s.CreatedDate,
                UpdatedDate = s.UpdatedDate
            };
        }

        private Setting SelectSettingByKey(List<Setting> allSettings, string key)
        {
            if (allSettings != null)
            {
                // Prefer the most recently updated row when duplicates exist for the same key.
                var result = allSettings
                    .Where(r => r.SettingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    .OrderByDescending(r => r.UpdatedDate)
                    .ThenByDescending(r => r.Id)
                    .FirstOrDefault();
                if (result != null)
                {
                    return result;
                }
            }

            var setting = new Setting();
            setting.SettingKey = key;
            setting.SettingValue = NULL_VALUE;
            return setting;
        }

        private Setting SelectSettingByKey(List<Setting> allSettings, string key, int language)
        {
            if (allSettings != null)
            {
                var result = allSettings
                    .Where(r => r.Lang == language && r.SettingKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(r => r.UpdatedDate)
                    .ThenByDescending(r => r.Id)
                    .FirstOrDefault();
                if (result != null)
                {
                    return result;
                }
            }

            var setting = new Setting();
            setting.SettingKey = key;
            setting.SettingValue = NULL_VALUE;
            setting.Lang = language;
            return setting;
        }

        private List<Setting> GetAllSettingsNoCache()
        {
            return SettingRepository.GetAllSettings();
        }

        private async Task<List<Setting>> GetAllSettingsNoCacheAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await SettingRepository.GetAllSettingsAsync(cancellationToken).ConfigureAwait(false);
        }

        private static string GetSettingKeyForProperty(string propertyName)
        {
            if (propertyName.StartsWith("RateLimit_", StringComparison.OrdinalIgnoreCase))
            {
                return propertyName.Replace('_', ':');
            }
            return propertyName;
        }

        private static Setting FindSettingForProperty(List<Setting> settings, List<Setting> allSettings, string propertyName)
        {
            string key = GetSettingKeyForProperty(propertyName);
            return settings.FirstOrDefault(r => r.SettingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase) || r.SettingKey.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))
                ?? allSettings.FirstOrDefault(r => r.SettingKey.Equals(key, StringComparison.InvariantCultureIgnoreCase) || r.SettingKey.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
        }

        public SystemSettingModel GetSystemSettingModel()
        {
            var result = new SystemSettingModel();

            Type type = result.GetType();
            // Prefer Description=SystemSettings, but fall back by SettingKey so seed rows
            // (human-readable Description) still populate the admin form before first save.
            List<Setting> allSettings = GetAllSettingsNoCache();
            List<Setting> Settings = allSettings.Where(r => Constants.SystemSettings.Equals(r.Description, StringComparison.InvariantCultureIgnoreCase)).ToList();
            // Loop over properties.
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                // Get name.
                string name = propertyInfo.Name;
                string key = GetSettingKeyForProperty(name);
                var setting = FindSettingForProperty(Settings, allSettings, name);

                string rawValue = (setting != null && setting.SettingValue != NULL_VALUE && !string.IsNullOrWhiteSpace(setting.SettingValue))
                    ? setting.SettingValue
                    : System.Configuration.ConfigurationManager.AppSettings[key] ?? System.Configuration.ConfigurationManager.AppSettings[name];

                if (rawValue != null)
                {
                    if (propertyInfo.PropertyType == typeof(int))
                    {
                        propertyInfo.SetValue(result, rawValue.ToInt(), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(string))
                    {
                        propertyInfo.SetValue(result, rawValue.ToStr(), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        propertyInfo.SetValue(result, rawValue.ToBool(), null);
                    }
                }
            }

            return result;
        }

        public async Task<SystemSettingModel> GetSystemSettingModelAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new SystemSettingModel();

            Type type = result.GetType();
            // Prefer Description=SystemSettings, but fall back by SettingKey so seed rows
            // (human-readable Description) still populate the admin form before first save.
            List<Setting> allSettings = await GetAllSettingsNoCacheAsync(cancellationToken).ConfigureAwait(false);
            List<Setting> Settings = allSettings
                .Where(r => Constants.SystemSettings.Equals(r.Description, StringComparison.InvariantCultureIgnoreCase)).ToList();
            // Loop over properties.
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                // Get name.
                string name = propertyInfo.Name;
                string key = GetSettingKeyForProperty(name);
                var setting = FindSettingForProperty(Settings, allSettings, name);

                string rawValue = (setting != null && setting.SettingValue != NULL_VALUE && !string.IsNullOrWhiteSpace(setting.SettingValue))
                    ? setting.SettingValue
                    : System.Configuration.ConfigurationManager.AppSettings[key] ?? System.Configuration.ConfigurationManager.AppSettings[name];

                if (rawValue != null)
                {
                    if (propertyInfo.PropertyType == typeof(int))
                    {
                        propertyInfo.SetValue(result, rawValue.ToInt(), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(string))
                    {
                        propertyInfo.SetValue(result, rawValue.ToStr(), null);
                    }
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        propertyInfo.SetValue(result, rawValue.ToBool(), null);
                    }
                }
            }

            return result;
        }

        public void SaveSystemSettingModel(SystemSettingModel settingModel)
        {
            if (settingModel == null)
            {
                throw new ArgumentException("SystemSettingModel cannot be null");
            }
            List<Setting> Settings = GetAllSettings();
            // Get type.
            Type type = settingModel.GetType();

            // Loop over properties.
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                // Get name.
                string name = propertyInfo.Name;
                string settingKey = GetSettingKeyForProperty(name);

                // Get value on the target instance.
                object value = propertyInfo.GetValue(settingModel, null);
                var setting = Settings.FirstOrDefault(r => r.SettingKey.Equals(settingKey, StringComparison.InvariantCultureIgnoreCase) || r.SettingKey.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (setting == null)
                {
                    var newSetting = new Setting();
                    newSetting.Name = settingKey;
                    newSetting.IsActive = true;
                    newSetting.SettingKey = settingKey;
                    newSetting.Description = Constants.SystemSettings;
                    newSetting.SettingValue = value.ToStr();
                    SaveOrEditEntity(newSetting);
                }
                else
                {
                    setting.SettingKey = settingKey;
                    setting.Description = Constants.SystemSettings;
                    setting.SettingValue = value.ToStr();
                    SaveOrEditEntity(setting);
                }
            }

            ClearCache();
        }

        public async Task SaveSystemSettingModelAsync(SystemSettingModel settingModel)
        {
            if (settingModel == null)
            {
                throw new ArgumentException("SystemSettingModel cannot be null");
            }
            List<Setting> Settings = await GetAllSettingsAsync().ConfigureAwait(false);
            // Get type.
            Type type = settingModel.GetType();

            // Loop over properties.
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                // Get name.
                string name = propertyInfo.Name;
                string settingKey = GetSettingKeyForProperty(name);

                // Get value on the target instance.
                object value = propertyInfo.GetValue(settingModel, null);
                var setting = Settings.FirstOrDefault(r => r.SettingKey.Equals(settingKey, StringComparison.InvariantCultureIgnoreCase) || r.SettingKey.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (setting == null)
                {
                    var newSetting = new Setting();
                    newSetting.Name = settingKey;
                    newSetting.IsActive = true;
                    newSetting.SettingKey = settingKey;
                    newSetting.Description = Constants.SystemSettings;
                    newSetting.SettingValue = value.ToStr();
                    await SaveOrEditEntityAsync(newSetting).ConfigureAwait(false);
                }
                else
                {
                    setting.SettingKey = settingKey;
                    setting.Description = Constants.SystemSettings;
                    setting.SettingValue = value.ToStr();
                    await SaveOrEditEntityAsync(setting).ConfigureAwait(false);
                }
            }

            ClearCache();
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

        public async Task<SettingModel> GetSettingModelAsync(int language)
        {
            var result = new SettingModel();

            Type type = result.GetType();
            List<Setting> Settings = (await GetAllSettingsAsync().ConfigureAwait(false))
                .Where(r => r.Lang == language && Constants.AdminSetting.Equals(r.Description, StringComparison.InvariantCultureIgnoreCase)).ToList();
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

        public void SaveSettingModel(SettingModel settingModel, int lang)
        {
            if (settingModel == null)
            {
                throw new ArgumentException("SettingModel cannot be null");
            }
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

        public async Task SaveSettingModelAsync(SettingModel settingModel, int lang)
        {
            if (settingModel == null)
            {
                throw new ArgumentException("SettingModel cannot be null");
            }
            List<Setting> Settings = await GetAllSettingsAsync().ConfigureAwait(false);
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
                    await SaveOrEditEntityAsync(newSetting).ConfigureAwait(false);
                }
                else
                {
                    setting.Description = Constants.AdminSetting;
                    setting.SettingValue = value.ToStr();
                    setting.Lang = lang;
                    await SaveOrEditEntityAsync(setting).ConfigureAwait(false);
                }
            }
        }

        public Dictionary<string, string> CreateShareableSocialMediaLinks(string link, string text, string imagefullPath)
        {
            var resultList = new Dictionary<String, String>();
            if (!string.IsNullOrWhiteSpace(link))
            {
                resultList.Add(Constants.SharePageUrl, link);
            }
            resultList.Add(Constants.LinkedinWebSiteLink, string.Format("https://www.linkedin.com/shareArticle?mini=true&url={0}&title={1}", WebUtility.UrlEncode(link), WebUtility.UrlEncode(text)));
            resultList.Add(Constants.FacebookWebSiteLink, string.Format("https://www.facebook.com/sharer/sharer.php?u={0}", WebUtility.UrlEncode(link)));
            resultList.Add(Constants.TwitterWebSiteLink, string.Format("https://twitter.com/intent/tweet?url={0}&text={1}", WebUtility.UrlEncode(link), WebUtility.UrlEncode(text)));
            resultList.Add(Constants.PinterestWebSiteLink, string.Format("https://pinterest.com/pin/create/button/?url={0}&media={2}&description={1}", WebUtility.UrlEncode(link), WebUtility.UrlEncode(text), WebUtility.UrlEncode(imagefullPath)));
            return resultList;
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

        public async Task<EmailAccount> GetEmailAccountAsync()
        {
            var emailAccount = new EmailAccount();
            emailAccount.Host = await GetSettingByKeyAsync(Constants.AdminEmailHost).ConfigureAwait(false);
            emailAccount.Password = await GetSettingByKeyAsync(Constants.AdminEmailPassword).ConfigureAwait(false);
            emailAccount.EnableSsl = (await GetSettingByKeyAsync(Constants.AdminEmailEnableSsl).ConfigureAwait(false)).ToBool();
            emailAccount.Port = (await GetSettingByKeyAsync(Constants.AdminEmailPort).ConfigureAwait(false)).ToInt();
            emailAccount.UseDefaultCredentials = (await GetSettingByKeyAsync(Constants.AdminEmailUseDefaultCredentials).ConfigureAwait(false)).ToBool();
            emailAccount.Email = await GetSettingByKeyAsync(Constants.AdminEmail).ConfigureAwait(false);
            emailAccount.Username = (await GetSettingByKeyAsync(Constants.AdminUserName).ConfigureAwait(false)).ToStr();
            emailAccount.Email = String.IsNullOrEmpty(emailAccount.Email) ? emailAccount.Username : emailAccount.Email;

            emailAccount.DisplayName = await GetSettingByKeyAsync(Constants.AdminEmailDisplayName).ConfigureAwait(false);
            emailAccount.DisplayName = String.IsNullOrEmpty(emailAccount.DisplayName) ? emailAccount.Username : emailAccount.DisplayName;

            return emailAccount;
        }
    }
}