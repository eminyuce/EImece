﻿using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Helpers
{
    public static class EnumHelper
    {
        public static Nullable<T> Parse<T>(String value, Boolean ignoreCase = true) where T : struct
        {
            return String.IsNullOrEmpty(value) ? null : (Nullable<T>)Enum.Parse(typeof(T), value, ignoreCase);
        }

        public static T[] GetValues<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        public static string EnumToString<T>(object value)
        {
            return Enum.GetName(typeof(T), value);
        }

        public static T Parse<T>(String value, Boolean ignoreCase, T defaultEnum) where T : struct
        {
            if ((!string.IsNullOrEmpty(value)) && (Enum.IsDefined(typeof(T), value)))
                return (T)EnumHelper.Parse<T>(value, ignoreCase);
            else
                return defaultEnum;
        }

        public static int GetEnumFromDescription(string description, Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentException();
            }

            foreach (var field in enumType.GetFields())
            {
                DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute == null)
                    continue;
                if (string.Equals(attribute.Description, description, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (int)field.GetValue(null);
                }
            }
            return 0;
        }

        public static string GetEnumDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return en.ToString();
        }

        public static List<SelectListItem> ToSelectList(this Enum enumValue)
        {
            return (from Enum e in Enum.GetValues(enumValue.GetType())
                    select new SelectListItem
                    {
                        Selected = e.Equals(enumValue),
                        Text = e.ToDescription(),
                        Value = e.ToString()
                    }).ToList();
        }

        public static List<SelectListItem> ToSelectList3(string cookieName)
        {
            var cultureCookie = HttpContext.Current.Request.Cookies[cookieName];
            string selected = cultureCookie == null ? "" : cultureCookie.Values[Constants.ELanguage];
            if (String.IsNullOrEmpty(selected))
            {
                selected = AppConfig.MainLanguage + "";
            }

            var values = ConfigureLanguagesFromAppConfigs();
            if (selected.ToInt() > 0)
            {
                return (from EImeceLanguage e in values
                        select new SelectListItem
                        {
                            Selected = selected.ToInt().Equals((int)e),
                            Text = e.GetDisplayValue(),
                            Value = ((int)e).ToStr()
                        }).ToList();
            }
            else
            {
                return (from EImeceLanguage e in values
                        select new SelectListItem
                        {
                            Selected = GetEnumDescription(e).Equals(selected),
                            Text = e.GetDisplayValue(),
                            Value = ((int)e).ToStr()
                        }).ToList();
            }
           
        }

        private static List<EImeceLanguage> ConfigureLanguagesFromAppConfigs()
        {
            var languagesText = AppConfig.ApplicationLanguages;
            var values = Enum.GetValues(typeof(EImeceLanguage)).Cast<EImeceLanguage>().ToList();
            if (String.IsNullOrEmpty(languagesText))
            {
                values.RemoveAll(r => r != EImeceLanguage.English);
            }
            else
            {
                List<EImeceLanguage> selectedLanguages = new List<EImeceLanguage>();
                var languages = Regex.Split(languagesText, @",").Select(r => r.Trim()).Where(s => !String.IsNullOrEmpty(s)).ToList();
                foreach (var lang in languages)
                {
                    try
                    {
                        var eImageLang = EnumHelper.GetEnumFromDescription(lang, typeof(EImeceLanguage));
                        selectedLanguages.Add((EImeceLanguage)eImageLang);
                    }
                    catch (Exception)
                    {
                    }
                }
                values = selectedLanguages;
            }

            return values;
        }

        public static string GetDisplayValue(this Enum value)
        {
            try
            {
                var fieldInfo = value.GetType().GetField(value.ToString());

                var descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];

                if (descriptionAttributes[0].ResourceType != null)
                    return lookupResource(descriptionAttributes[0].ResourceType, descriptionAttributes[0].Name);

                if (descriptionAttributes == null) return string.Empty;
                return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return String.Empty;
            }
        }

        public static IList<string> GetNames(Enum value)
        {
            return value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public).Select(fi => fi.Name).ToList();
        }

        private static string lookupResource(Type resourceManagerProvider, string resourceKey)
        {
            foreach (PropertyInfo staticProperty in resourceManagerProvider.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (staticProperty.PropertyType == typeof(System.Resources.ResourceManager))
                {
                    System.Resources.ResourceManager resourceManager = (System.Resources.ResourceManager)staticProperty.GetValue(null, null);
                    return resourceManager.GetString(resourceKey);
                }
            }

            return resourceKey; // Fallback with the key name
        }

        public static string ToDescription(this Enum value)
        {
            try
            {
                var attributes = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
                return attributes.Length > 0 ? attributes[0].Description : value.ToString();
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

        public static IEnumerable<SelectListItem> ToSelectListWithId(this Enum enumValue)
        {
            return from Enum e in Enum.GetValues(enumValue.GetType())
                   select new SelectListItem
                   {
                       Selected = e.Equals(enumValue),
                       Text = e.ToDescription(),
                       Value = e.ToStr()
                   };
        }

        public static List<EImeceLanguage> GetLanguageEnumListFromWebConfig()
        {
            List<EImeceLanguage> selectedLanguages = new List<EImeceLanguage>();
         
            var languagesText = AppConfig.ApplicationLanguages;
            if (String.IsNullOrEmpty(languagesText))
            {
                selectedLanguages.Add((EImeceLanguage)AppConfig.MainLanguage);
            }
            else
            {
                var languages = Regex.Split(languagesText, @",").Select(r => r.Trim()).Where(s => !String.IsNullOrEmpty(s)).ToList();
                foreach (var lang in languages)
                {
                    try
                    {
                        var eImageLang = EnumHelper.GetEnumFromDescription(lang, typeof(EImeceLanguage));
                        selectedLanguages.Add((EImeceLanguage)eImageLang);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return selectedLanguages;
        }
    }
}