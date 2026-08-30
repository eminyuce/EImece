using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Web.Helpers
{
    public static class EnumWebExtensions
    {
        public static List<SelectListItem> ToSelectList(this Enum enumValue)
        {
            if (enumValue == null) return new List<SelectListItem>();

            return (from Enum e in Enum.GetValues(enumValue.GetType())
                    select new SelectListItem
                    {
                        Selected = e.Equals(enumValue),
                        Text = e.ToDescription(),
                        Value = e.ToString()
                    }).ToList();
        }

        public static IEnumerable<SelectListItem> ToSelectListWithId(this Enum enumValue)
        {
            if (enumValue == null) return Enumerable.Empty<SelectListItem>();

            return from Enum e in Enum.GetValues(enumValue.GetType())
                   select new SelectListItem
                   {
                       Selected = e.Equals(enumValue),
                       Text = e.ToDescription(),
                       Value = e.ToStr()
                   };
        }

        public static List<SelectListItem> ToSelectList3(string cookieName)
        {
            var cultureCookie = HttpContext.Current?.Request?.Cookies[cookieName];
            string selected = cultureCookie == null ? "" : cultureCookie.Values[Constants.ELanguage];
            if (string.IsNullOrEmpty(selected))
            {
                selected = AppConfig.MainLanguage + "";
            }

            var values = EnumHelper.GetLanguageEnumListFromWebConfig();
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
                            Selected = EnumHelper.GetEnumDescription(e).Equals(selected),
                            Text = e.GetDisplayValue(),
                            Value = ((int)e).ToStr()
                        }).ToList();
            }
        }
    }
}
