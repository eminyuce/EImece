using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class ContentLanguageSettingsHelperTests
    {
        [TestMethod]
        public void Parse_NullOrEmpty_DefaultsToTurkishOnly()
        {
            AssertTurkishOnly(ContentLanguageSettingsHelper.Parse(null));
            AssertTurkishOnly(ContentLanguageSettingsHelper.Parse(""));
            AssertTurkishOnly(ContentLanguageSettingsHelper.Parse("   "));
        }

        [TestMethod]
        public void Parse_EnglishOnly_ForcesEnglish()
        {
            var settings = ContentLanguageSettingsHelper.Parse("en-US");

            Assert.IsFalse(settings.TurkishEnabled);
            Assert.IsTrue(settings.EnglishEnabled);
            Assert.IsFalse(settings.IsBilingual);
            Assert.AreEqual(EImeceLanguage.English, settings.DefaultLanguage);
            Assert.AreEqual(Constants.EN_US_CULTURE_INFO, settings.ForcedCultureName);
            Assert.AreEqual("en-US", settings.SerializedCultures);
        }

        [TestMethod]
        public void Parse_TurkishOnly_ForcesTurkish()
        {
            var settings = ContentLanguageSettingsHelper.Parse("tr-TR");
            AssertTurkishOnly(settings);
        }

        [TestMethod]
        public void Parse_Both_EnablesBilingualWithTurkishDefault()
        {
            var settings = ContentLanguageSettingsHelper.Parse("tr-TR,en-US");

            Assert.IsTrue(settings.TurkishEnabled);
            Assert.IsTrue(settings.EnglishEnabled);
            Assert.IsTrue(settings.IsBilingual);
            Assert.AreEqual(EImeceLanguage.Turkish, settings.DefaultLanguage);
            Assert.AreEqual("tr-TR,en-US", settings.SerializedCultures);
            Assert.AreEqual(2, settings.EnabledLanguages.Count);
        }

        [TestMethod]
        public void Parse_IgnoresUnsupportedCulturesAndFallsBackToTurkish()
        {
            AssertTurkishOnly(ContentLanguageSettingsHelper.Parse("ru-RU,de-DE"));
        }

        [TestMethod]
        public void Parse_AcceptsLegacyNumericAndNameTokens()
        {
            var both = ContentLanguageSettingsHelper.Parse("1,2");
            Assert.IsTrue(both.IsBilingual);

            var english = ContentLanguageSettingsHelper.Parse("English");
            Assert.AreEqual(EImeceLanguage.English, english.DefaultLanguage);
        }

        [TestMethod]
        public void FromCheckboxes_NeitherSelected_DefaultsToTurkish()
        {
            AssertTurkishOnly(ContentLanguageSettingsHelper.FromCheckboxes(false, false));
        }

        [TestMethod]
        public void Serialize_MatchesExpectedCsv()
        {
            Assert.AreEqual("tr-TR", ContentLanguageSettingsHelper.Serialize(true, false));
            Assert.AreEqual("en-US", ContentLanguageSettingsHelper.Serialize(false, true));
            Assert.AreEqual("tr-TR,en-US", ContentLanguageSettingsHelper.Serialize(true, true));
            Assert.AreEqual(Constants.DefaultSupportedContentLanguages, ContentLanguageSettingsHelper.Serialize(false, false));
        }

        [TestMethod]
        public void ResolveStorefrontCulture_SingleLanguage_IgnoresCookies()
        {
            var englishOnly = ContentLanguageSettingsHelper.Parse("en-US");
            Assert.AreEqual("en-US", englishOnly.ForcedCultureName);
            Assert.IsFalse(englishOnly.IsCultureEnabled("tr-TR"));
        }

        private static void AssertTurkishOnly(ContentLanguageSettings settings)
        {
            Assert.IsTrue(settings.TurkishEnabled);
            Assert.IsFalse(settings.EnglishEnabled);
            Assert.IsFalse(settings.IsBilingual);
            Assert.AreEqual(EImeceLanguage.Turkish, settings.DefaultLanguage);
            Assert.AreEqual(1, settings.DefaultLanguageId);
            Assert.AreEqual(Constants.TR, settings.ForcedCultureName);
            Assert.AreEqual("tr-TR", settings.SerializedCultures);
        }
    }
}
