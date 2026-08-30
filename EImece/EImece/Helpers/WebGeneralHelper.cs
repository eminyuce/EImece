using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Helpers
{
    public static class WebGeneralHelper
    {
        public static SelectList GetStaticCountries()
        {
            string[] countriesArray = new string[] { "Türkiye", "Amerika Birleşik Devletleri", "Kanada", "Almanya", "Diğerleri" };
            return new SelectList(countriesArray.Select(r => new SelectListItem { Selected = false, Text = r, Value = r }).ToList(), "Value", "Text", "Türkiye");
        }

        public static SelectList GetYears()
        {
            var listItems = GetYearList();
            var sli = new SelectListItem { Text = "All", Value = "0" };
            listItems.Insert(0, sli);
            return new SelectList(listItems, "Value", "Text");
        }

        private static List<SelectListItem> GetYearList()
        {
            var listItems = new List<SelectListItem>();
            int i = DateTime.Now.Year;
            for (i = i - 1; i <= DateTime.Now.Year + 10; i++)
            {
                String year = i.ToString();
                listItems.Add(new SelectListItem { Text = year, Value = year });
            }
            return listItems;
        }

        public static SelectList GetMonths()
        {
            var listItems = GetMonthList();
            var sli = new SelectListItem { Text = "All", Value = "0" };
            listItems.Insert(0, sli);
            return new SelectList(listItems, "Value", "Text");
        }

        private static List<SelectListItem> GetMonthList()
        {
            var listItems = new List<SelectListItem>();
            var months = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
            for (int i = 0; i < months.Length; i++)
            {
                if (!String.IsNullOrEmpty(months[i]))
                {
                    int m = i + 1;
                    listItems.Add(new SelectListItem { Text = months[i], Value = m.ToString() });
                }
            }
            return listItems;
        }

        public static string GetIpAddress()
        {
            var context = HttpContext.Current;
            if (context?.Request != null)
            {
                return (context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ??
                        context.Request.ServerVariables["REMOTE_ADDR"])?.Split(',')[0].Trim() ?? "127.0.0.1";
            }
            return "127.0.0.1";
        }

        public static void SetCultureCookie(HttpResponseBase response, string cultureCookieName, string cultureName = "")
        {
            if (response == null) return;
            if (string.IsNullOrEmpty(cultureName))
            {
                cultureName = EnumHelper.GetEnumDescription(((EImeceLanguage)AppConfig.MainLanguage));
            }
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            if (response.Cookies[cultureCookieName] != null)
            {
                response.Cookies[cultureCookieName].Value = cultureName;
            }
        }

        public static string GetCultureCookie(HttpResponseBase response, string cultureCookieName)
        {
            if (response == null) return string.Empty;
            string cultureName = null;
            HttpCookie cultureCookie = response.Cookies[cultureCookieName];
            if (cultureCookie != null)
            {
                cultureName = cultureCookie.Value;
            }
            if (string.IsNullOrEmpty(cultureName))
            {
                cultureName = EnumHelper.GetEnumDescription(((EImeceLanguage)AppConfig.MainLanguage));
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            return cultureName;
        }

        public static string GetClientIpAddress(HttpRequestBase request)
        {
            if (request == null) return "0.0.0.0";
            try
            {
                var userHostAddress = request.UserHostAddress;
                var xForwardedFor = request.ServerVariables["X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(xForwardedFor))
                    return userHostAddress;

                var publicForwardingIps = xForwardedFor.Split(',').Where(ip => !GeneralHelper.IsPrivateIpAddress(ip.Trim())).ToList();
                return publicForwardingIps.Any() ? publicForwardingIps.Last().Trim() : userHostAddress;
            }
            catch
            {
                return "0.0.0.0";
            }
        }

        public static string GetSiteDomain(HttpContextBase httpContextBase)
        {
            if (httpContextBase?.Request == null) return string.Empty;
            HttpRequestBase request = httpContextBase.Request;
            string domainName = request.Url.Scheme + Uri.SchemeDelimiter + request.Url.Host +
                                     (request.Url.IsDefaultPort ? "" : ":" + request.Url.Port);
            return GeneralHelper.GetDomainPart(domainName);
        }
    }
}
