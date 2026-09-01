using EImece.Domain;
using EImece.Domain.Abstractions;
using EImece.Domain.Caching;
using EImece.Domain.Services.IServices;
using EImece.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading.Tasks;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class ApplicationCacheClearerTests
    {
        [TestMethod]
        public void ClearHttpRuntime_NullClearer_ReturnsZero()
        {
            Assert.AreEqual(0, ApplicationCacheClearer.ClearHttpRuntime(null));
        }

        [TestMethod]
        public void MemoryCacheProvider_ClearAll_DropsProviderKeys()
        {
            var cache = new MemoryCacheProvider(TestNullLoggers.Create<MemoryCacheProvider>());
            var key = "test:clearall:" + Guid.NewGuid().ToString("N");
            cache.Set(key, 1, CachePolicy.Absolute(60));

            var removed = cache.ClearAll();
            Assert.IsTrue(removed >= 1);
            Assert.IsFalse(cache.Get(key, out int _));
        }

        [TestMethod]
        public void InvalidateWebsiteLogo_DropsLogoBytesAndAsksToRemoveOutputCachePath()
        {
            var cache = new MemoryCacheProvider(TestNullLoggers.Create<MemoryCacheProvider>());
            cache.Set(CacheKeys.WebSiteLogoImage, "new-logo-bytes", CachePolicy.Absolute(3600));
            cache.Set(CacheKeys.WebSiteLogoImageLegacy, "old-logo-bytes", CachePolicy.Absolute(3600));

            var settings = new SettingServiceClearCacheProxy();
            var clearer = new RecordingOutputCacheClearer();
            AdminCacheMaintenance.InvalidateWebsiteLogo(settings.Service, cache, clearer);

            Assert.IsTrue(settings.ClearCacheCalled);
            Assert.IsFalse(cache.Get(CacheKeys.WebSiteLogoImage, out string _));
            Assert.IsFalse(cache.Get(CacheKeys.WebSiteLogoImageLegacy, out string _));
            Assert.AreEqual(Constants.LogoImagePath, clearer.RemovedPath);
        }

        private sealed class RecordingOutputCacheClearer : IHttpRuntimeCacheClearer
        {
            public string RemovedPath { get; private set; }

            public int ClearHttpRuntimeCache()
            {
                return 0;
            }

            public void RemoveOutputCacheItem(string virtualPath)
            {
                RemovedPath = virtualPath;
            }
        }

        private sealed class SettingServiceClearCacheProxy : RealProxy
        {
            public SettingServiceClearCacheProxy() : base(typeof(ISettingService))
            {
            }

            public bool ClearCacheCalled { get; private set; }

            public ISettingService Service => (ISettingService)GetTransparentProxy();

            public override IMessage Invoke(IMessage msg)
            {
                var call = (IMethodCallMessage)msg;
                if (string.Equals(call.MethodName, "ClearCache", StringComparison.Ordinal))
                {
                    ClearCacheCalled = true;
                }

                object defaultResult = null;
                if (call.MethodBase is MethodInfo mi && mi.ReturnType != typeof(void))
                {
                    if (mi.ReturnType == typeof(Task))
                    {
                        defaultResult = Task.CompletedTask;
                    }
                    else if (mi.ReturnType.IsGenericType && mi.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        var innerType = mi.ReturnType.GetGenericArguments()[0];
                        var defaultInner = innerType.IsValueType ? Activator.CreateInstance(innerType) : null;
                        defaultResult = typeof(Task).GetMethod("FromResult").MakeGenericMethod(innerType)
                            .Invoke(null, new[] { defaultInner });
                    }
                    else if (mi.ReturnType.IsValueType)
                    {
                        defaultResult = Activator.CreateInstance(mi.ReturnType);
                    }
                }

                return new ReturnMessage(defaultResult, null, 0, call.LogicalCallContext, call);
            }
        }
    }
}
