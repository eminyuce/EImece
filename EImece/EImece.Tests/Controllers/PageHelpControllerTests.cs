using EImece.Areas.Admin.Controllers;
using EImece.Controllers.Api;
using EImece.Domain.Abstractions;
using EImece.Domain.Caching;
using EImece.Domain.Helpers;
using EImece.Domain.Services.IServices;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace EImece.Tests.Controllers
{
    [TestClass]
    public class PageHelpControllerTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            PageHelpHelper.EvictCache();
        }

        internal class InterfaceMockProxy<T> : RealProxy
        {
            private readonly Func<string, object[], object> _handler;

            public InterfaceMockProxy(Func<string, object[], object> handler = null) : base(typeof(T))
            {
                _handler = handler;
            }

            public override IMessage Invoke(IMessage msg)
            {
                var call = (IMethodCallMessage)msg;
                var mi = call.MethodBase as MethodInfo;
                if (_handler != null)
                {
                    try
                    {
                        var custom = _handler(call.MethodName, call.Args);
                        if (custom != null)
                        {
                            return new ReturnMessage(custom, null, 0, call.LogicalCallContext, call);
                        }
                    }
                    catch (Exception ex)
                    {
                        return new ReturnMessage(ex, call);
                    }
                }

                // If calling GetOrAdd / GetOrAddAsync without custom mock handler, invoke the factory delegate if present
                if ((call.MethodName == "GetOrAdd" || call.MethodName == "GetOrAddAsync") && call.Args != null)
                {
                    var factory = call.Args.OfType<Delegate>().FirstOrDefault();
                    if (factory != null)
                    {
                        try
                        {
                            var val = factory.DynamicInvoke();
                            return new ReturnMessage(val, null, 0, call.LogicalCallContext, call);
                        }
                        catch { }
                    }
                }

                object defaultResult = null;
                if (mi != null && mi.ReturnType != typeof(void))
                {
                    if (mi.ReturnType == typeof(Task))
                    {
                        defaultResult = Task.CompletedTask;
                    }
                    else if (mi.ReturnType.IsGenericType && mi.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        var innerType = mi.ReturnType.GetGenericArguments()[0];
                        var defaultInner = innerType.IsValueType ? Activator.CreateInstance(innerType) : null;
                        defaultResult = typeof(Task).GetMethod("FromResult").MakeGenericMethod(innerType).Invoke(null, new[] { defaultInner });
                    }
                    else if (mi.ReturnType.IsValueType)
                    {
                        defaultResult = Activator.CreateInstance(mi.ReturnType);
                    }
                }
                return new ReturnMessage(defaultResult, null, 0, call.LogicalCallContext, call);
            }

            public T Service => (T)GetTransparentProxy();
        }

        internal static T Mock<T>(Func<string, object[], object> handler = null) => new InterfaceMockProxy<T>(handler).Service;

        private DashboardController CreateController(ISettingService settingService)
        {
            var productService = Mock<IProductService>();
            var productCategoryService = Mock<IProductCategoryService>();
            var storyService = Mock<IStoryService>();
            var storyCategoryService = Mock<IStoryCategoryService>();
            var menuService = Mock<IMenuService>();
            var memoryCacheProvider = Mock<IEimeceCacheProvider>();
            var httpRuntimeCacheClearer = Mock<IHttpRuntimeCacheClearer>();
            var logger = NullLogger<DashboardController>.Instance;

            return new DashboardController(
                settingService,
                productService,
                productCategoryService,
                storyService,
                storyCategoryService,
                menuService,
                memoryCacheProvider,
                httpRuntimeCacheClearer,
                logger
            );
        }

        [TestMethod]
        public async Task GetPageHelp_WhenKeyIsEmpty_ReturnsFailure()
        {
            var settingService = Mock<ISettingService>();
            var controller = CreateController(settingService);

            var result = await controller.GetPageHelp("") as JsonResult;
            Assert.IsNotNull(result);

            var jObj = Newtonsoft.Json.Linq.JObject.FromObject(result.Data);
            Assert.IsFalse((bool)jObj["success"]);
        }

        [TestMethod]
        public async Task GetPageHelp_WhenKeyExistsInSettingService_ReturnsSettingContent()
        {
            const string expectedHtml = "<p>Custom database help content</p>";
            var settingService = Mock<ISettingService>((method, args) =>
            {
                if (method == nameof(ISettingService.GetSettingByKeyAsync) && args.Length > 0 && (string)args[0] == "CustomPage_help/info_section")
                {
                    return Task.FromResult(expectedHtml);
                }
                return null;
            });

            var controller = CreateController(settingService);
            var result = await controller.GetPageHelp("CustomPage_help/info_section") as JsonResult;

            Assert.IsNotNull(result);
            var jObj = Newtonsoft.Json.Linq.JObject.FromObject(result.Data);
            Assert.IsTrue((bool)jObj["success"]);
            Assert.IsTrue((bool)jObj["hasContent"]);
            Assert.AreEqual(expectedHtml, (string)jObj["content"]);
        }

        [TestMethod]
        public async Task GetPageHelp_WhenKeyNotExistsInSettingService_FallsBackToDefaultContent()
        {
            var settingService = Mock<ISettingService>((method, args) =>
            {
                if (method == nameof(ISettingService.GetSettingByKeyAsync))
                {
                    return Task.FromResult(string.Empty);
                }
                return null;
            });

            var controller = CreateController(settingService);
            var result = await controller.GetPageHelp("SystemUsers_help/info_section") as JsonResult;

            Assert.IsNotNull(result);
            var jObj = Newtonsoft.Json.Linq.JObject.FromObject(result.Data);
            Assert.IsTrue((bool)jObj["success"]);
            Assert.IsTrue((bool)jObj["hasContent"]);
            StringAssert.Contains((string)jObj["content"], "sistem kullanıcılarını yönetirsiniz");
        }

        [TestMethod]
        public async Task GetPageHelp_WhenUnknownKeyAndNoFallback_ReturnsHasContentFalse()
        {
            var settingService = Mock<ISettingService>((method, args) =>
            {
                if (method == nameof(ISettingService.GetSettingByKeyAsync))
                {
                    return Task.FromResult(string.Empty);
                }
                return null;
            });

            var controller = CreateController(settingService);
            var result = await controller.GetPageHelp("NonExistentPage_help/info_section") as JsonResult;

            Assert.IsNotNull(result);
            var jObj = Newtonsoft.Json.Linq.JObject.FromObject(result.Data);
            Assert.IsTrue((bool)jObj["success"]);
            Assert.IsFalse((bool)jObj["hasContent"]);
            Assert.AreEqual(string.Empty, (string)jObj["content"]);
        }

        private AdminSettingsController CreateAdminSettingsController(ISettingService settingService, IEimeceCacheProvider memoryCache = null)
        {
            var emailSender = Mock<EImece.Domain.Helpers.EmailHelper.IEmailSender>();
            var cache = memoryCache ?? Mock<IEimeceCacheProvider>();
            var dataExport = Mock<EImece.Domain.Services.ExportImport.IDataExportService>();
            var logger = NullLogger<AdminSettingsController>.Instance;

            return new AdminSettingsController(
                settingService,
                emailSender,
                cache,
                dataExport,
                logger
            );
        }

        [TestMethod]
        public async Task GetPageHelpList_ReturnsPredefinedAndCustomPages()
        {
            var customSetting = new EImece.Domain.Entities.Setting
            {
                SettingKey = "SpecialAudit_help/info_section",
                Name = "Özel Denetim",
                SettingValue = "<p>Özel içerik</p>",
                UpdatedDate = DateTime.UtcNow
            };

            var settingService = Mock<ISettingService>((method, args) =>
            {
                if (method == nameof(ISettingService.GetAllAsync))
                {
                    return Task.FromResult(new System.Collections.Generic.List<EImece.Domain.Entities.Setting> { customSetting });
                }
                return null;
            });

            var controller = CreateAdminSettingsController(settingService);
            var result = await controller.GetPageHelpList(System.Threading.CancellationToken.None) as JsonResult;

            Assert.IsNotNull(result);
            var jObj = Newtonsoft.Json.Linq.JObject.FromObject(result.Data);
            Assert.IsTrue((bool)jObj["success"]);

            var items = jObj["items"] as Newtonsoft.Json.Linq.JArray;
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Count > 10);

            var foundCustom = items.FirstOrDefault(i => (string)i["Key"] == "SpecialAudit_help/info_section");
            Assert.IsNotNull(foundCustom);
            Assert.IsTrue((bool)foundCustom["IsCustomized"]);
            Assert.AreEqual("Özel Denetim", (string)foundCustom["PageName"]);

            var foundUsers = items.FirstOrDefault(i => (string)i["Key"] == "SystemUsers_help/info_section");
            Assert.IsNotNull(foundUsers);
            Assert.AreEqual("Sistem Kullanıcıları", (string)foundUsers["PageName"]);
        }

        [TestMethod]
        public async Task GetPageHelpDetail_WhenKeyIsPredefined_ReturnsPredefinedDetail()
        {
            var settingService = Mock<ISettingService>((method, args) =>
            {
                if (method == nameof(ISettingService.GetSettingObjectByKeyFromDbAsync))
                {
                    return Task.FromResult<EImece.Domain.Entities.Setting>(null);
                }
                return null;
            });

            var controller = CreateAdminSettingsController(settingService);
            var result = await controller.GetPageHelpDetail("SystemUsers_help/info_section", System.Threading.CancellationToken.None) as JsonResult;

            Assert.IsNotNull(result);
            var jObj = Newtonsoft.Json.Linq.JObject.FromObject(result.Data);
            Assert.IsTrue((bool)jObj["success"]);
            Assert.AreEqual("SystemUsers_help/info_section", (string)jObj["key"]);
            Assert.AreEqual("Sistem Kullanıcıları", (string)jObj["name"]);
            Assert.IsFalse((bool)jObj["isCustomized"]);
            StringAssert.Contains((string)jObj["content"], "sistem kullanıcılarını yönetirsiniz");
        }

        [TestMethod]
        public async Task SavePageHelp_SavesEntityAndClearsCache()
        {
            EImece.Domain.Entities.Setting savedSetting = null;
            bool cacheCleared = false;

            var settingService = Mock<ISettingService>((method, args) =>
            {
                if (method == nameof(ISettingService.GetSettingObjectByKeyFromDbAsync))
                {
                    return Task.FromResult<EImece.Domain.Entities.Setting>(null);
                }
                if (method == nameof(ISettingService.SaveOrEditEntityAsync))
                {
                    savedSetting = (EImece.Domain.Entities.Setting)args[0];
                    return Task.FromResult(savedSetting);
                }
                if (method == nameof(ISettingService.ClearCache))
                {
                    cacheCleared = true;
                    return null;
                }
                return null;
            });

            var cache = Mock<IEimeceCacheProvider>((method, args) =>
            {
                if (method == nameof(IEimeceCacheProvider.ClearAll))
                {
                    cacheCleared = true;
                    return 1;
                }
                return null;
            });

            var controller = CreateAdminSettingsController(settingService, cache);
            var result = await controller.SavePageHelp(
                "CustomOrders_help/info_section",
                "Sipariş Yönetimi",
                "<p>Sipariş detayları...</p>",
                System.Threading.CancellationToken.None
            ) as JsonResult;

            Assert.IsNotNull(result);
            var jObj = Newtonsoft.Json.Linq.JObject.FromObject(result.Data);
            Assert.IsTrue((bool)jObj["success"]);
            Assert.IsNotNull(savedSetting);
            Assert.AreEqual("CustomOrders_help/info_section", savedSetting.SettingKey);
            Assert.AreEqual("Sipariş Yönetimi", savedSetting.Name);
            Assert.AreEqual("<p>Sipariş detayları...</p>", savedSetting.SettingValue);
            Assert.IsTrue(cacheCleared);
        }

        [TestMethod]
        public async Task DeletePageHelp_WhenSettingExists_DeletesEntityAndClearsCache()
        {
            var existing = new EImece.Domain.Entities.Setting
            {
                SettingKey = "SystemUsers_help/info_section",
                SettingValue = "<p>Custom Users Info</p>"
            };
            bool entityDeleted = false;
            bool cacheCleared = false;

            var settingService = Mock<ISettingService>((method, args) =>
            {
                if (method == nameof(ISettingService.GetSettingObjectByKeyFromDbAsync))
                {
                    return Task.FromResult(existing);
                }
                if (method == nameof(ISettingService.DeleteEntityAsync))
                {
                    entityDeleted = true;
                    return Task.FromResult(true);
                }
                if (method == nameof(ISettingService.ClearCache))
                {
                    cacheCleared = true;
                    return null;
                }
                return null;
            });

            var controller = CreateAdminSettingsController(settingService);
            var result = await controller.DeletePageHelp("SystemUsers_help/info_section", System.Threading.CancellationToken.None) as JsonResult;

            Assert.IsNotNull(result);
            var jObj = Newtonsoft.Json.Linq.JObject.FromObject(result.Data);
            Assert.IsTrue((bool)jObj["success"]);
            Assert.IsTrue(entityDeleted);
            Assert.IsTrue(cacheCleared);
            StringAssert.Contains((string)jObj["defaultContent"], "sistem kullanıcılarını yönetirsiniz");
        }

        [TestMethod]
        public void HasHelpContent_WhenPredefinedPage_ReturnsTrue()
        {
            Assert.IsTrue(PageHelpHelper.HasHelpContent("change-password"));
            Assert.IsTrue(PageHelpHelper.HasHelpContent("Users_ChangePassword_help/info_section"));
            Assert.IsTrue(PageHelpHelper.HasHelpContent("SystemUsers_help/info_section"));
            Assert.IsTrue(PageHelpHelper.HasHelpContent("users"));
        }

        [TestMethod]
        public void HasHelpContent_WhenUnknownPage_ReturnsFalse()
        {
            Assert.IsFalse(PageHelpHelper.HasHelpContent("some-random-unknown-page"));
            Assert.IsFalse(PageHelpHelper.HasHelpContent("unknown_page_help/info_section"));
            Assert.IsFalse(PageHelpHelper.HasHelpContent(""));
            Assert.IsFalse(PageHelpHelper.HasHelpContent(null));
        }

        [TestMethod]
        public void IsValidContent_WhenEmptyOrHtmlTagsOnly_ReturnsFalse()
        {
            Assert.IsFalse(PageHelpHelper.IsValidContent(null));
            Assert.IsFalse(PageHelpHelper.IsValidContent(""));
            Assert.IsFalse(PageHelpHelper.IsValidContent("   "));
            Assert.IsFalse(PageHelpHelper.IsValidContent("RETURN-NULL-VALUE"));
            Assert.IsFalse(PageHelpHelper.IsValidContent("null"));
            Assert.IsFalse(PageHelpHelper.IsValidContent("<p></p>"));
            Assert.IsFalse(PageHelpHelper.IsValidContent("<p><br></p>"));
            Assert.IsFalse(PageHelpHelper.IsValidContent("<p>&nbsp;</p>"));
            Assert.IsFalse(PageHelpHelper.IsValidContent("<div><span> </span></div>"));
        }

        [TestMethod]
        public void IsValidContent_WhenValidTextOrHtml_ReturnsTrue()
        {
            Assert.IsTrue(PageHelpHelper.IsValidContent("Hello World"));
            Assert.IsTrue(PageHelpHelper.IsValidContent("<p>Yardım metni</p>"));
            Assert.IsTrue(PageHelpHelper.IsValidContent("<p><img src='test.png'/></p>"));
        }

        [TestMethod]
        public void HasHelpContent_WhenSettingInDbIsEmpty_ReturnsFalse()
        {
            var mockSettingService = (ISettingService)new InterfaceMockProxy<ISettingService>((method, args) =>
            {
                if (method == "GetSettingObjectByKeyFromDb")
                {
                    return new EImece.Domain.Entities.Setting
                    {
                        SettingKey = (string)args[0],
                        SettingValue = "<p><br></p>",
                        Name = "Kullanıcılar",
                        IsActive = true
                    };
                }
                if (method == "GetAllActiveSettings")
                {
                    return new System.Collections.Generic.List<EImece.Domain.Entities.Setting>
                    {
                        new EImece.Domain.Entities.Setting
                        {
                            SettingKey = "SystemUsers_help/info_section",
                            SettingValue = "<p><br></p>",
                            Name = "Kullanıcılar",
                            IsActive = true
                        }
                    };
                }
                return null;
            }).GetTransparentProxy();

            var mockCache = (IEimeceCacheProvider)new InterfaceMockProxy<IEimeceCacheProvider>().GetTransparentProxy();

            bool result = PageHelpHelper.HasHelpContent("SystemUsers_help/info_section", mockSettingService, mockCache);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void EvictCache_ClearsCacheAndAllowsRefresh()
        {
            // Initial call caches the dictionary
            var item = PageHelpHelper.GetHelpItem("change-password");
            Assert.IsNotNull(item);
            Assert.IsTrue(item.HasContent);

            // Evict
            PageHelpHelper.EvictCache();

            // Next call reloads cache
            var reloaded = PageHelpHelper.GetHelpItem("change-password");
            Assert.IsNotNull(reloaded);
            Assert.IsTrue(reloaded.HasContent);
        }
    }

    [TestClass]
    public class HelpApiControllerTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            PageHelpHelper.EvictCache();
        }

        private HelpApiController CreateApiController(ISettingService settingService)
        {
            var logger = NullLogger<HelpApiController>.Instance;
            return new HelpApiController(settingService, logger);
        }

        private static Newtonsoft.Json.Linq.JObject GetContentAsJObject(IHttpActionResult result)
        {
            Assert.IsNotNull(result, "IHttpActionResult must not be null");
            var contentProperty = result.GetType().GetProperty("Content");
            Assert.IsNotNull(contentProperty, $"Result of type {result.GetType().Name} must have a Content property");
            var content = contentProperty.GetValue(result);
            Assert.IsNotNull(content, "Content value must not be null");
            return Newtonsoft.Json.Linq.JObject.FromObject(content);
        }

        [TestMethod]
        public void PageHelpHelper_ResolveKey_MapsSlugsAndFallbacks()
        {
            Assert.AreEqual("Users_ChangePassword_help/info_section", PageHelpHelper.ResolveKey("change-password"));
            Assert.AreEqual("SystemUsers_help/info_section", PageHelpHelper.ResolveKey("users"));
            Assert.AreEqual("SystemUsers_help/info_section", PageHelpHelper.ResolveKey("admin/users"));
            Assert.AreEqual("ProductPage_help/info_section", PageHelpHelper.ResolveKey("products"));
            Assert.AreEqual("ProductCategories_help/info_section", PageHelpHelper.ResolveKey("categories"));
            Assert.AreEqual("Dashboard_Index_help/info_section", PageHelpHelper.ResolveKey("Dashboard_Index"));
            Assert.AreEqual("Custom_help/info_section", PageHelpHelper.ResolveKey("Custom_help/info_section"));
        }

        [TestMethod]
        public async Task GetHelp_WhenKeyIsEmpty_ReturnsBadRequest()
        {
            var settingService = PageHelpControllerTests.Mock<ISettingService>();
            var controller = CreateApiController(settingService);

            var result = await controller.GetHelp("") as BadRequestErrorMessageResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("pageKey cannot be empty.", result.Message);
        }

        [TestMethod]
        public async Task GetHelp_WhenDbSettingExists_ReturnsDbSettingAndFormattedDate()
        {
            var updateDate = new DateTime(2026, 9, 14, 10, 0, 0, DateTimeKind.Utc);
            var dbSetting = new EImece.Domain.Entities.Setting
            {
                SettingKey = "Users_ChangePassword_help/info_section",
                Name = "Şifre Güncelleme",
                SettingValue = "<p>Yeni şifrenizi en az 8 karakter yapın.</p>",
                UpdatedDate = updateDate
            };

            var settingService = PageHelpControllerTests.Mock<ISettingService>((method, args) =>
            {
                if (method == nameof(ISettingService.GetSettingObjectByKeyFromDbAsync))
                {
                    return Task.FromResult(dbSetting);
                }
                return null;
            });

            var controller = CreateApiController(settingService);
            var result = await controller.GetHelp("change-password");

            var jObj = GetContentAsJObject(result);
            Assert.IsTrue((bool)jObj["success"]);
            Assert.AreEqual("change-password", (string)jObj["pageKey"]);
            Assert.AreEqual("Users_ChangePassword_help/info_section", (string)jObj["resolvedKey"]);
            Assert.AreEqual("Şifre Güncelleme", (string)jObj["title"]);
            Assert.AreEqual("<p>Yeni şifrenizi en az 8 karakter yapın.</p>", (string)jObj["content"]);
            Assert.AreEqual("14.09.2026", (string)jObj["lastUpdated"]);
            Assert.IsTrue((bool)jObj["hasContent"]);
        }

        [TestMethod]
        public async Task GetHelp_WhenDbSettingMissing_ReturnsPredefinedFallback()
        {
            var settingService = PageHelpControllerTests.Mock<ISettingService>((method, args) =>
            {
                if (method == nameof(ISettingService.GetSettingObjectByKeyFromDbAsync))
                {
                    return Task.FromResult<EImece.Domain.Entities.Setting>(null);
                }
                return null;
            });

            var controller = CreateApiController(settingService);
            var result = await controller.GetHelp("change-password");

            var jObj = GetContentAsJObject(result);
            Assert.IsTrue((bool)jObj["success"]);
            Assert.AreEqual("Users_ChangePassword_help/info_section", (string)jObj["resolvedKey"]);
            Assert.AreEqual("Kullanıcı Şifre Değiştirme", (string)jObj["title"]);
            StringAssert.Contains((string)jObj["content"], "şifresini güvenli şekilde sıfırlama");
            Assert.IsNull((string)jObj["lastUpdated"]);
            Assert.IsTrue((bool)jObj["hasContent"]);
        }

        [TestMethod]
        public async Task GetHelp_WhenUnknownPageAndNoFallback_ReturnsEmptyContent()
        {
            var settingService = PageHelpControllerTests.Mock<ISettingService>((method, args) =>
            {
                if (method == nameof(ISettingService.GetSettingObjectByKeyFromDbAsync))
                {
                    return Task.FromResult<EImece.Domain.Entities.Setting>(null);
                }
                return null;
            });

            var controller = CreateApiController(settingService);
            var result = await controller.GetHelp("unknown-random-slug-xyz");

            var jObj = GetContentAsJObject(result);
            Assert.IsTrue((bool)jObj["success"]);
            Assert.IsFalse((bool)jObj["hasContent"]);
            Assert.AreEqual(string.Empty, (string)jObj["content"]);
        }

        [TestMethod]
        public async Task GetHelpQuery_WhenParamIsEmpty_ReturnsBadRequest()
        {
            var settingService = PageHelpControllerTests.Mock<ISettingService>();
            var controller = CreateApiController(settingService);

            var result = await controller.GetHelpQuery(null) as BadRequestErrorMessageResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("pageKey query parameter is required.", result.Message);
        }

        [TestMethod]
        public async Task GetHelpQuery_WhenValid_ReturnsSameResultAsGetHelp()
        {
            var settingService = PageHelpControllerTests.Mock<ISettingService>((method, args) =>
            {
                if (method == nameof(ISettingService.GetSettingObjectByKeyFromDbAsync))
                {
                    return Task.FromResult<EImece.Domain.Entities.Setting>(null);
                }
                return null;
            });

            var controller = CreateApiController(settingService);
            var result = await controller.GetHelpQuery("change-password");

            var jObj = GetContentAsJObject(result);
            Assert.IsTrue((bool)jObj["success"]);
            Assert.AreEqual("Users_ChangePassword_help/info_section", (string)jObj["resolvedKey"]);
        }

        [TestMethod]
        public void SubmitFeedback_WhenRequestIsNull_ReturnsBadRequest()
        {
            var settingService = PageHelpControllerTests.Mock<ISettingService>();
            var controller = CreateApiController(settingService);

            var result = controller.SubmitFeedback(null) as BadRequestErrorMessageResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Invalid feedback request.", result.Message);
        }

        [TestMethod]
        public void SubmitFeedback_WhenPageKeyIsEmpty_ReturnsBadRequest()
        {
            var settingService = PageHelpControllerTests.Mock<ISettingService>();
            var controller = CreateApiController(settingService);

            var result = controller.SubmitFeedback(new HelpFeedbackRequest { PageKey = "   " }) as BadRequestErrorMessageResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Invalid feedback request.", result.Message);
        }

        [TestMethod]
        public void SubmitFeedback_WhenValid_ReturnsSuccessAndFeedbackState()
        {
            var settingService = PageHelpControllerTests.Mock<ISettingService>();
            var controller = CreateApiController(settingService);

            var request = new HelpFeedbackRequest
            {
                PageKey = "change-password",
                IsHelpful = true,
                Comment = "Yardımcı oldu, teşekkürler!"
            };

            var result = controller.SubmitFeedback(request);

            var jObj = GetContentAsJObject(result);
            Assert.IsTrue((bool)jObj["success"]);
            Assert.IsTrue((bool)jObj["isHelpful"]);
            Assert.AreEqual("Users_ChangePassword_help/info_section", (string)jObj["pageKey"]);
            StringAssert.Contains((string)jObj["message"], "teşekkür");
        }
    }
}
