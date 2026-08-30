using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Content language (“içerik dili”) is independent of Admin Panel UI language.
    /// Only Turkish (tr-TR) and English (en-US) are supported.
    /// </summary>
    public sealed class ContentLanguageSettings
    {
        internal ContentLanguageSettings(bool turkishEnabled, bool englishEnabled)
        {
            if (!turkishEnabled && !englishEnabled)
            {
                turkishEnabled = true;
            }

            TurkishEnabled = turkishEnabled;
            EnglishEnabled = englishEnabled;
            IsBilingual = turkishEnabled && englishEnabled;
            DefaultLanguage = englishEnabled && !turkishEnabled
                ? EImeceLanguage.English
                : EImeceLanguage.Turkish;
            DefaultLanguageId = (int)DefaultLanguage;
            ForcedCultureName = DefaultLanguage == EImeceLanguage.English
                ? Constants.EN_US_CULTURE_INFO
                : Constants.TR;
            SerializedCultures = ContentLanguageSettingsHelper.Serialize(turkishEnabled, englishEnabled);

            var enabled = new List<EImeceLanguage>(2);
            if (turkishEnabled)
            {
                enabled.Add(EImeceLanguage.Turkish);
            }
            if (englishEnabled)
            {
                enabled.Add(EImeceLanguage.English);
            }
            EnabledLanguages = enabled;
        }

        public bool TurkishEnabled { get; }
        public bool EnglishEnabled { get; }
        public bool IsBilingual { get; }
        public EImeceLanguage DefaultLanguage { get; }
        public int DefaultLanguageId { get; }
        public string ForcedCultureName { get; }
        public string SerializedCultures { get; }
        public IReadOnlyList<EImeceLanguage> EnabledLanguages { get; }

        public bool IsLanguageEnabled(EImeceLanguage language)
        {
            if (language == EImeceLanguage.Turkish)
            {
                return TurkishEnabled;
            }
            if (language == EImeceLanguage.English)
            {
                return EnglishEnabled;
            }
            return false;
        }

        public bool IsCultureEnabled(string culture)
        {
            var parsed = EnumHelper.ParseLanguage(culture);
            return parsed.HasValue && IsLanguageEnabled(parsed.Value);
        }
    }

    public static class ContentLanguageSettingsHelper
    {
        public static ContentLanguageSettings TurkishOnly { get; } = new ContentLanguageSettings(true, false);

        public static string Serialize(bool turkishEnabled, bool englishEnabled)
        {
            if (!turkishEnabled && !englishEnabled)
            {
                return Constants.DefaultSupportedContentLanguages;
            }
            if (turkishEnabled && englishEnabled)
            {
                return "tr-TR,en-US";
            }
            return turkishEnabled ? Constants.TR : Constants.EN_US_CULTURE_INFO;
        }

        public static ContentLanguageSettings Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return TurkishOnly;
            }

            bool turkish = false;
            bool english = false;
            var parts = raw.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var parsed = EnumHelper.ParseLanguage(part);
                if (!parsed.HasValue)
                {
                    continue;
                }
                if (parsed.Value == EImeceLanguage.Turkish)
                {
                    turkish = true;
                }
                else if (parsed.Value == EImeceLanguage.English)
                {
                    english = true;
                }
            }

            if (!turkish && !english)
            {
                return TurkishOnly;
            }

            return new ContentLanguageSettings(turkish, english);
        }

        public static ContentLanguageSettings FromCheckboxes(bool turkishEnabled, bool englishEnabled)
        {
            if (!turkishEnabled && !englishEnabled)
            {
                return TurkishOnly;
            }
            return new ContentLanguageSettings(turkishEnabled, englishEnabled);
        }

        /// <summary>
        /// Reads the DB-backed system setting. Missing or empty values default to Turkish only.
        /// </summary>
        public static ContentLanguageSettings GetCurrent()
        {
            try
            {
                var settingService = DependencyResolver.Current?.GetService(typeof(ISettingService)) as ISettingService;
                var raw = settingService?.GetSettingByKey(Constants.SupportedContentLanguages);
                return Parse(raw);
            }
            catch
            {
                return TurkishOnly;
            }
        }

        public static string ResolveStorefrontCulture(string languageCookieValue, string cultureCookieELanguage)
        {
            var current = GetCurrent();
            if (!current.IsBilingual)
            {
                return current.ForcedCultureName;
            }

            if (!string.IsNullOrWhiteSpace(languageCookieValue) && current.IsCultureEnabled(languageCookieValue))
            {
                var parsed = EnumHelper.ParseLanguage(languageCookieValue);
                if (parsed.HasValue)
                {
                    return EnumHelper.GetEnumDescription(parsed.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(cultureCookieELanguage) && current.IsCultureEnabled(cultureCookieELanguage))
            {
                var parsed = EnumHelper.ParseLanguage(cultureCookieELanguage);
                if (parsed.HasValue)
                {
                    return EnumHelper.GetEnumDescription(parsed.Value);
                }
            }

            return current.ForcedCultureName;
        }
    }
}
