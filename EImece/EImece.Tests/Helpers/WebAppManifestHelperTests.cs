using EImece.Domain;
using EImece.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class WebAppManifestHelperTests
    {
        [TestMethod]
        public void BuildJson_UsesCompanyNameAndMetaDescription()
        {
            var json = WebAppManifestHelper.BuildJson(
                "Acme Shop",
                "Home Title",
                "Quality products for everyone",
                null,
                "#1789F9",
                "www.example.com");

            var obj = JObject.Parse(json);
            Assert.AreEqual("Acme Shop", (string)obj["name"]);
            Assert.AreEqual("Acme Shop", (string)obj["short_name"]);
            Assert.AreEqual("Quality products for everyone", (string)obj["description"]);
            Assert.AreEqual(Constants.DefaultManifestStartUrl, (string)obj["start_url"]);
            Assert.AreEqual(Constants.DefaultManifestDisplay, (string)obj["display"]);
            Assert.AreEqual(Constants.DefaultManifestOrientation, (string)obj["orientation"]);
            Assert.AreEqual("#1789F9", (string)obj["theme_color"]);
            Assert.AreEqual(Constants.DefaultManifestBackgroundColor, (string)obj["background_color"]);
        }

        [TestMethod]
        public void BuildJson_FallsBackWhenSettingsMissing()
        {
            var json = WebAppManifestHelper.BuildJson(
                "  ",
                null,
                "",
                "not-a-color",
                "",
                "localhost:81");

            var obj = JObject.Parse(json);
            Assert.AreEqual("localhost", (string)obj["name"]);
            Assert.AreEqual("localhost", (string)obj["short_name"]);
            Assert.AreEqual("localhost", (string)obj["description"]);
            Assert.AreEqual(Constants.DefaultThemeColor, (string)obj["theme_color"]);
        }

        [TestMethod]
        public void BuildJson_PrefersSettingsThemeColorOverFallback()
        {
            var json = WebAppManifestHelper.BuildJson(
                "Shop",
                null,
                null,
                "#067a36",
                "#ffffff",
                "example.com");

            var obj = JObject.Parse(json);
            Assert.AreEqual("#067a36", (string)obj["theme_color"]);
        }

        [TestMethod]
        public void BuildJson_IncludesExpectedPngIcons()
        {
            var json = WebAppManifestHelper.BuildJson("Shop", null, null, null, "#1789F9", "example.com");
            var obj = JObject.Parse(json);
            var icons = (JArray)obj["icons"];
            var sizes = icons.Select(i => (string)i["sizes"]).ToArray();

            CollectionAssert.AreEqual(
                new[] { "36x36", "48x48", "72x72", "96x96", "144x144", "192x192", "256x256", "384x384", "512x512" },
                sizes);
            Assert.AreEqual("/android-chrome-512x512.png", (string)icons.Last()["src"]);
            Assert.IsTrue(icons.All(i => (string)i["type"] == "image/png"));
        }

        [TestMethod]
        public void ToShortName_TruncatesAtWordBoundary()
        {
            Assert.AreEqual("Acme", WebAppManifestHelper.ToShortName("Acme Trading Co"));
            Assert.AreEqual("Short", WebAppManifestHelper.ToShortName("Short"));
            Assert.AreEqual(Constants.DefaultManifestFallbackName, WebAppManifestHelper.ToShortName("  "));
        }

        [TestMethod]
        public void IsValidHexColor_AcceptsCommonForms()
        {
            Assert.IsTrue(WebAppManifestHelper.IsValidHexColor("#fff"));
            Assert.IsTrue(WebAppManifestHelper.IsValidHexColor("#1789F9"));
            Assert.IsTrue(WebAppManifestHelper.IsValidHexColor("#067a36ff"));
            Assert.IsFalse(WebAppManifestHelper.IsValidHexColor("1789F9"));
            Assert.IsFalse(WebAppManifestHelper.IsValidHexColor("blue"));
            Assert.IsFalse(WebAppManifestHelper.IsValidHexColor(""));
        }
    }
}
