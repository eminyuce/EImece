using EImece.Domain.Helpers.EmailHelper;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class MailTemplateModelInspectorTests
    {
        private const string PasswordResetTemplate = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <title>Şifre Sıfırlama Talebi</title>
</head>
<body>
    <div class=""container"">
        <img src=""@Model.WebSiteIconUrl"" alt=""Şirket Logosu"" class=""logo"">
        <h2>Şifre Sıfırlama Talebi</h2>
        <p>Merhaba <strong>@Model.Email</strong>,</p>
        <p><strong>@Model.CompanyName</strong> üzerinde hesabınız için bir şifre sıfırlama talebi alındı.</p>
        <a href=""@Model.ForgotPasswordLink"" class=""button"">Şifremi Sıfırla</a>
        <p><a href=""@Model.ForgotPasswordLink"">@Model.ForgotPasswordLink</a></p>
        <p class=""footer"">Teşekkürler, <br><strong>@Model.CompanyName Yönetimi</strong></p>
    </div>
</body>
</html>";

        [TestMethod]
        public void ExtractPropertyPaths_FindsModelPropertiesAndIgnoresDuplicates()
        {
            var paths = MailTemplateModelInspector.ExtractPropertyPaths(PasswordResetTemplate);

            CollectionAssert.AreEquivalent(
                new[] { "WebSiteIconUrl", "Email", "CompanyName", "ForgotPasswordLink" },
                paths);
        }

        [TestMethod]
        public void ExtractPropertyPaths_IgnoresEscapedAtSigns()
        {
            var paths = MailTemplateModelInspector.ExtractPropertyPaths("Konu: @@Model.OrderNumber ve @Model.CustomerName");

            CollectionAssert.AreEqual(new[] { "CustomerName" }, paths);
        }

        [TestMethod]
        public void ExtractPropertyPaths_SupportsIndexerAndNestedPaths()
        {
            var template = "@Model[\"CompanyName\"] @Model.ContactUs.Email @Model['BaseUrl']";
            var paths = MailTemplateModelInspector.ExtractPropertyPaths(template);

            CollectionAssert.AreEquivalent(
                new[] { "CompanyName", "ContactUs.Email", "BaseUrl" },
                paths);
        }

        [TestMethod]
        public void InferValueKind_DetectsCommonPropertyTypes()
        {
            Assert.AreEqual(MailTemplateValueKind.Email, MailTemplateModelInspector.InferValueKind("Email"));
            Assert.AreEqual(MailTemplateValueKind.Url, MailTemplateModelInspector.InferValueKind("ForgotPasswordLink"));
            Assert.AreEqual(MailTemplateValueKind.ImageUrl, MailTemplateModelInspector.InferValueKind("WebSiteIconUrl"));
            Assert.AreEqual(MailTemplateValueKind.Phone, MailTemplateModelInspector.InferValueKind("CompanyPhoneNumber"));
            Assert.AreEqual(MailTemplateValueKind.String, MailTemplateModelInspector.InferValueKind("CompanyName"));
        }

        [TestMethod]
        public void GenerateSampleValue_UsesContextAndKnownPropertyNames()
        {
            var context = new MailTemplateDummyDataContext
            {
                BaseUrl = "https://shop.test",
                CompanyName = "Test Co",
                RecipientEmail = "admin@shop.test",
                LogoUrl = "https://shop.test/images/logo.jpg"
            };

            Assert.AreEqual("admin@shop.test", MailTemplateModelInspector.GenerateSampleValue("Email", context));
            Assert.AreEqual("Test Co", MailTemplateModelInspector.GenerateSampleValue("CompanyName", context));
            Assert.AreEqual("https://shop.test/images/logo.jpg", MailTemplateModelInspector.GenerateSampleValue("WebSiteIconUrl", context));
            StringAssert.Contains(
                MailTemplateModelInspector.GenerateSampleValue("ForgotPasswordLink", context),
                "https://shop.test/account/resetpassword");
        }

        [TestMethod]
        public void BuildProperties_IsExtensibleForUnknownFields()
        {
            var properties = MailTemplateModelInspector.BuildProperties(
                new[] { "PromoCode", "SupportEmail" },
                MailTemplateDummyDataContext.CreateDefaults());

            Assert.AreEqual(2, properties.Count);
            Assert.AreEqual("PromoCode", properties[0].Path);
            Assert.AreEqual("SupportEmail", properties[1].Path);
            Assert.AreEqual("Email", properties.Single(p => p.Path == "SupportEmail").ValueKind);
            Assert.IsFalse(string.IsNullOrWhiteSpace(properties.Single(p => p.Path == "PromoCode").SampleValue));
        }

        [TestMethod]
        public void Render_ReplacesPasswordResetModelValues()
        {
            var model = new Dictionary<string, string>
            {
                { "WebSiteIconUrl", "https://shop.test/logo.png" },
                { "Email", "user@shop.test" },
                { "CompanyName", "Shop Test" },
                { "ForgotPasswordLink", "https://shop.test/reset" }
            };

            var result = MailTemplateModelInspector.Render("Şifre — @Model.CompanyName", PasswordResetTemplate, model);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual("Şifre — Shop Test", result.Subject);
            StringAssert.Contains(result.Body, "user@shop.test");
            StringAssert.Contains(result.Body, "Shop Test");
            StringAssert.Contains(result.Body, "https://shop.test/reset");
            StringAssert.Contains(result.Body, "https://shop.test/logo.png");
            StringAssert.DoesNotMatch(result.Body, new System.Text.RegularExpressions.Regex(@"@Model\."));
        }

        [TestMethod]
        public void Render_SupportsNestedProperties()
        {
            var result = MailTemplateModelInspector.Render(
                null,
                "<p>@Model.ContactUs.Name - @Model.ContactUs.Email</p>",
                new Dictionary<string, string>
                {
                    { "ContactUs.Name", "Ada" },
                    { "ContactUs.Email", "ada@example.com" }
                });

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.Body, "Ada");
            StringAssert.Contains(result.Body, "ada@example.com");
        }

        [TestMethod]
        public async Task SendTestEmailAsync_RejectsMissingAndInvalidRecipient()
        {
            var service = new MailTemplateTestService();

            var missing = await service.SendTestEmailAsync(null);
            Assert.IsFalse(missing.Success);
            StringAssert.Contains(missing.Message, "boş");

            var invalid = await service.SendTestEmailAsync(new SendMailTemplateTestRequest
            {
                RecipientEmail = "not-an-email"
            });
            Assert.IsFalse(invalid.Success);
            StringAssert.Contains(invalid.Message, "Geçerli bir alıcı");
        }
    }
}
