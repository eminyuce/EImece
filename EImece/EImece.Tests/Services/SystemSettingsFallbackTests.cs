using EImece.Domain;
using EImece.Domain.Caching;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Helpers;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Repositories;
using EImece.Domain.Services;
using EImece.Infrastructure.Designs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Tests.Services
{
    [TestClass]
    public class SystemSettingsFallbackTests
    {
        private FakeSettingRepository _repository;
        private SettingService _settingService;

        [TestInitialize]
        public void Setup()
        {
            _repository = new FakeSettingRepository();
            _settingService = new SettingService(_repository);
            _settingService.DataCachingProvider = new MemoryCacheProvider();
            _settingService.ClearCache();
        }

        [TestCleanup]
        public void Cleanup()
        {
            AppConfig.SettingResolver = null;
            _settingService?.ClearCache();
        }

        [TestMethod]
        public void GetSettingByKey_WhenDatabaseRowExists_ReturnsDatabaseValue()
        {
            // Arrange
            _repository.InMemSettings.Add(new Setting
            {
                Id = 1,
                SettingKey = Constants.ActiveDesign,
                SettingValue = "Modern",
                IsActive = true,
                Description = Constants.SystemSettings
            });

            // Act
            string value = _settingService.GetSettingByKey(Constants.ActiveDesign);

            // Assert
            Assert.AreEqual("Modern", value);
        }

        [TestMethod]
        public void GetSettingByKey_WhenDatabaseRowMissing_FallsBackToWebConfig()
        {
            // Arrange - DB empty for this key, Web.config has RateLimit:Login:Limit = 5
            ConfigurationManager.AppSettings["RateLimit:Login:Limit"] = "5";

            // Act
            string value = _settingService.GetSettingByKey("RateLimit:Login:Limit");

            // Assert
            Assert.AreEqual("5", value);
        }

        [TestMethod]
        public void GetSettingByKey_WhenDatabaseValueIsEmptyOrNullValue_FallsBackToWebConfig()
        {
            // Arrange - DB row exists but value is whitespace
            _repository.InMemSettings.Add(new Setting
            {
                Id = 2,
                SettingKey = "RateLimit:Search:Limit",
                SettingValue = "   ",
                IsActive = true
            });

            ConfigurationManager.AppSettings["RateLimit:Search:Limit"] = "30";

            // Act
            string value = _settingService.GetSettingByKey("RateLimit:Search:Limit");

            // Assert
            Assert.AreEqual("30", value);
        }

        [TestMethod]
        public async Task GetSettingByKeyAsync_WhenDatabaseRowExists_ReturnsDatabaseValue()
        {
            // Arrange
            _repository.InMemSettings.Add(new Setting
            {
                Id = 3,
                SettingKey = Constants.AllowSearchEngineIndexing,
                SettingValue = "true",
                IsActive = true,
                Description = Constants.SystemSettings
            });

            // Act
            string value = await _settingService.GetSettingByKeyAsync(Constants.AllowSearchEngineIndexing);

            // Assert
            Assert.AreEqual("true", value);
        }

        [TestMethod]
        public void AppConfig_WithSettingResolver_PrefersDatabaseOverWebConfig()
        {
            // Arrange
            _repository.InMemSettings.Add(new Setting
            {
                Id = 4,
                SettingKey = Constants.ImageUploadJpegQuality,
                SettingValue = "92",
                IsActive = true,
                Description = Constants.SystemSettings
            });

            AppConfig.SettingResolver = key => _settingService.GetSettingByKey(key);

            // Act & Assert
            Assert.AreEqual(92, AppConfig.ImageUploadJpegQuality);
        }

        [TestMethod]
        public void AppConfig_WithSettingResolver_FallsBackToWebConfigWhenDbMissing()
        {
            // Arrange
            AppConfig.SettingResolver = key => _settingService.GetSettingByKey(key);

            // Act & Assert (GridPageSizeNumber in Web.config is 100)
            Assert.AreEqual(100, AppConfig.GridPageSizeNumber);
        }

        [TestMethod]
        public void SystemSettingModel_RoundTrip_MapsColonKeysAndSavesSuccessfully()
        {
            // Arrange
            var model = new SystemSettingModel
            {
                ActiveDesign = "Modern",
                AllowSearchEngineIndexing = true,
                IsSiteUnderConstruction = false,
                RateLimit_Enabled = true,
                RateLimit_Login_Limit = 15,
                RateLimit_Login_WindowMinutes = 20,
                RateLimit_Search_Limit = 50,
                RateLimit_Search_WindowMinutes = 2,
                ThemeColor = "#1789F9",
                ManifestBackgroundColor = "#ffffff",
                ManifestDisplay = "standalone",
                ManifestOrientation = "portrait",
                ManifestStartUrl = "/",
                ManifestFallbackName = "EImece PWA",
                ManifestShortNameMaxLength = 10,
                ImageUploadMaxWidth = 2560,
                ImageUploadMaxHeight = 1440,
                ImageUploadJpegQuality = 88,
                ImageUploadPreferWebP = true,
                ImageUploadWebPQuality = 85,
                ImageUploadSaveWebPSidecar = true,
                ImageUploadThumbMaxWidth = 600,
                ImageUploadThumbMaxHeight = 600,
                ImageUploadThumbJpegQuality = 70,
                ImageUploadKeepOriginalIfSmaller = true,
                PaymentProvider = "Iyzico",
                IyzicoEnabledInstallments = "1,2,3,6",
                BuyerIdentityNumber = "11111111111",
                CaptchaProvider = "Recaptcha",
                RecaptchaSiteKey = "public-recaptcha-key-test"
            };

            // Act
            _settingService.SaveSystemSettingModel(model);

            // Assert saved keys in repo
            var loginLimitSetting = _repository.InMemSettings.FirstOrDefault(s => s.SettingKey == "RateLimit:Login:Limit");
            Assert.IsNotNull(loginLimitSetting, "RateLimit:Login:Limit setting must be saved with colon delimiter");
            Assert.AreEqual("15", loginLimitSetting.SettingValue);

            var activeDesignSetting = _repository.InMemSettings.FirstOrDefault(s => s.SettingKey == "ActiveDesign");
            Assert.IsNotNull(activeDesignSetting);
            Assert.AreEqual("Modern", activeDesignSetting.SettingValue);

            var loadedModel = _settingService.GetSystemSettingModel();
            Assert.AreEqual("Modern", loadedModel.ActiveDesign);
            Assert.IsTrue(loadedModel.AllowSearchEngineIndexing);
            Assert.AreEqual(15, loadedModel.RateLimit_Login_Limit);
            Assert.AreEqual(20, loadedModel.RateLimit_Login_WindowMinutes);
            Assert.AreEqual(88, loadedModel.ImageUploadJpegQuality);
            Assert.IsTrue(loadedModel.ImageUploadPreferWebP);
            Assert.AreEqual("Recaptcha", loadedModel.CaptchaProvider);
            Assert.AreEqual("public-recaptcha-key-test", loadedModel.RecaptchaSiteKey);
        }

        [TestMethod]
        public void SeoSettings_And_ConfigDesignProvider_ReflectDynamicDbChanges()
        {
            // Arrange
            _repository.InMemSettings.Add(new Setting
            {
                Id = 10,
                SettingKey = Constants.AllowSearchEngineIndexing,
                SettingValue = "true",
                IsActive = true,
                Description = Constants.SystemSettings
            });
            _repository.InMemSettings.Add(new Setting
            {
                Id = 11,
                SettingKey = Constants.ActiveDesign,
                SettingValue = "Modern",
                IsActive = true,
                Description = Constants.SystemSettings
            });

            AppConfig.SettingResolver = key => _settingService.GetSettingByKey(key);
            var designProvider = new ConfigDesignProvider();

            // Act & Assert
            Assert.IsTrue(SeoSettings.AllowIndexing);
            Assert.AreEqual("Modern", designProvider.GetActiveDesign());
        }

        private class FakeDbContextProxy : RealProxy
        {
            public FakeDbContextProxy() : base(typeof(IEImeceContext))
            {
            }

            public override IMessage Invoke(IMessage msg)
            {
                var call = (IMethodCallMessage)msg;
                object defaultResult = null;
                if (call.MethodBase is MethodInfo mi && mi.ReturnType != typeof(void))
                {
                    if (mi.ReturnType.IsValueType)
                    {
                        defaultResult = Activator.CreateInstance(mi.ReturnType);
                    }
                }
                return new ReturnMessage(defaultResult, null, 0, call.LogicalCallContext, call);
            }

            public IEImeceContext Context => (IEImeceContext)GetTransparentProxy();
        }

        private class FakeSettingRepository : SettingRepository
        {
            public List<Setting> InMemSettings { get; } = new List<Setting>();
            private int _idCounter = 100;

            public FakeSettingRepository() : base(new FakeDbContextProxy().Context)
            {
            }

            public override List<Setting> GetAllSettings()
            {
                return InMemSettings.ToList();
            }

            public override Task<List<Setting>> GetAllSettingsAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(InMemSettings.ToList());
            }

            public override List<Setting> GetAllActiveSettings()
            {
                return InMemSettings.Where(s => s.IsActive).ToList();
            }

            public override int SaveOrEdit(Setting entity)
            {
                var existing = InMemSettings.FirstOrDefault(s => s.Id == entity.Id || s.SettingKey.Equals(entity.SettingKey, StringComparison.OrdinalIgnoreCase));
                if (existing != null && existing != entity)
                {
                    existing.SettingValue = entity.SettingValue;
                    existing.Description = entity.Description;
                    existing.IsActive = entity.IsActive;
                    existing.UpdatedDate = DateTime.UtcNow;
                }
                else if (existing == null)
                {
                    if (entity.Id == 0) entity.Id = ++_idCounter;
                    InMemSettings.Add(entity);
                }
                return 1;
            }

            public override Task<int> SaveOrEditAsync(Setting entity)
            {
                SaveOrEdit(entity);
                return Task.FromResult(1);
            }
        }
    }
}
