using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public abstract class BaseController : Controller
    {
        [Inject]
        public ISettingService SettingService { get; set; }

        public void CreateLanguageCookie(EImeceLanguage selectedLanguage, string cookieName)
        {
            String cultureName = EnumHelper.GetEnumDescription(selectedLanguage);
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            HttpCookie cultureCookie = new HttpCookie(cookieName);
            cultureCookie.Values[Constants.ELanguage] = ((int)selectedLanguage) + "";
            cultureCookie.Values[Constants.LastVisit] = DateTime.Now.ToString();
            cultureCookie.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Add(cultureCookie);
        }

        protected int CurrentLanguage
        {
            get
            {
                HttpCookie cultureCookie = Request.Cookies[Constants.CultureCookieName];
                if (cultureCookie != null)
                {
                    return cultureCookie.Values[Constants.ELanguage].ToInt();
                }
                else
                {
                    return AppConfig.MainLanguage;
                }
            }
        }
    }
}