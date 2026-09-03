using EImece.Domain.Configuration;
using EImece.Domain.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EImece.Tests.Configuration
{
    [TestClass]
    public class IyzicoOptionsAndEnumsTests
    {
        [TestMethod]
        public void IyzicoOptions_DefaultsAndProperties()
        {
            var options = new IyzicoOptions();
            Assert.AreEqual(string.Empty, options.ApiKey);
            Assert.AreEqual(string.Empty, options.SecretKey);
            Assert.AreEqual("https://sandbox-api.iyzipay.com", options.BaseUrl);
            Assert.IsFalse(options.IsConfigured);

            options.ApiKey = "test-key";
            Assert.IsFalse(options.IsConfigured);

            options.SecretKey = "test-secret";
            Assert.IsTrue(options.IsConfigured);

            options.BaseUrl = "https://api.iyzipay.com";
            Assert.AreEqual("https://api.iyzipay.com", options.BaseUrl);
        }

        [TestMethod]
        public void IyzicoOptions_FromAppConfigAndReset()
        {
            IyzicoOptions.ResetForTests();
            var options = IyzicoOptions.FromAppConfig();
            Assert.IsNotNull(options);

            // Re-calling returns the same cached instance
            var options2 = IyzicoOptions.FromAppConfig();
            Assert.AreSame(options, options2);

            // Resetting invalidates the cache
            IyzicoOptions.ResetForTests();
            var options3 = IyzicoOptions.FromAppConfig();
            Assert.AreNotSame(options, options3);
        }

        [TestMethod]
        public void IyzicoPaymentStatus_AllEnumsDefined()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(IyzicoPaymentStatus), "SUCCESS"));
            Assert.IsTrue(Enum.IsDefined(typeof(IyzicoPaymentStatus), "FAILURE"));
            Assert.IsTrue(Enum.IsDefined(typeof(IyzicoPaymentStatus), "INIT_THREEDS"));
            Assert.IsTrue(Enum.IsDefined(typeof(IyzicoPaymentStatus), "CALLBACK_THREEDS"));
            Assert.IsTrue(Enum.IsDefined(typeof(IyzicoPaymentStatus), "BKM_POS_SELECTED"));
            Assert.IsTrue(Enum.IsDefined(typeof(IyzicoPaymentStatus), "CALLBACK_PECCO"));

            Assert.AreEqual(IyzicoPaymentStatus.SUCCESS, (IyzicoPaymentStatus)Enum.Parse(typeof(IyzicoPaymentStatus), "SUCCESS"));
            Assert.AreEqual(IyzicoPaymentStatus.FAILURE, (IyzicoPaymentStatus)Enum.Parse(typeof(IyzicoPaymentStatus), "FAILURE"));
        }
    }
}
