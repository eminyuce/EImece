using EImece.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class PaymentGatewayConfigTests
    {
        [TestMethod]
        public void IyzicoCredentials_ReadFromEnvironmentVariablesWhenSet()
        {
            var origApiKey = Environment.GetEnvironmentVariable("EIMECE_IYZICO_API_KEY");
            var origSecretKey = Environment.GetEnvironmentVariable("EIMECE_IYZICO_SECRET_KEY");

            try
            {
                Environment.SetEnvironmentVariable("EIMECE_IYZICO_API_KEY", "test-env-api-key");
                Environment.SetEnvironmentVariable("EIMECE_IYZICO_SECRET_KEY", "test-env-secret-key");

                Assert.AreEqual("test-env-api-key", AppConfig.IyzicoApiKey);
                Assert.AreEqual("test-env-secret-key", AppConfig.IyzicoSecretKey);
                Assert.IsTrue(AppConfig.HasConfiguredIyzicoCredentials);
            }
            finally
            {
                Environment.SetEnvironmentVariable("EIMECE_IYZICO_API_KEY", origApiKey);
                Environment.SetEnvironmentVariable("EIMECE_IYZICO_SECRET_KEY", origSecretKey);
            }
        }

        [TestMethod]
        public void ValidatePaymentGatewayCredentials_FailsClosedWhenCredentialsMissing()
        {
            var origApiKey = Environment.GetEnvironmentVariable("EIMECE_IYZICO_API_KEY");
            var origSecretKey = Environment.GetEnvironmentVariable("EIMECE_IYZICO_SECRET_KEY");
            var origAppConfigApiKey = ConfigurationManager.AppSettings["IyzicoApiKey"];
            var origAppConfigSecretKey = ConfigurationManager.AppSettings["IyzicoSecretKey"];

            try
            {
                Environment.SetEnvironmentVariable("EIMECE_IYZICO_API_KEY", "");
                Environment.SetEnvironmentVariable("EIMECE_IYZICO_SECRET_KEY", "");
                ConfigurationManager.AppSettings["IyzicoApiKey"] = "";
                ConfigurationManager.AppSettings["IyzicoSecretKey"] = "";

                AppConfig.ValidatePaymentGatewayCredentials();
                Assert.Fail("Expected ConfigurationErrorsException when gateway credentials are missing.");
            }
            catch (ConfigurationErrorsException ex)
            {
                StringAssert.Contains(ex.Message, "Payment gateway credentials are missing");
            }
            finally
            {
                Environment.SetEnvironmentVariable("EIMECE_IYZICO_API_KEY", origApiKey);
                Environment.SetEnvironmentVariable("EIMECE_IYZICO_SECRET_KEY", origSecretKey);
                ConfigurationManager.AppSettings["IyzicoApiKey"] = origAppConfigApiKey;
                ConfigurationManager.AppSettings["IyzicoSecretKey"] = origAppConfigSecretKey;
            }
        }
    }
}
