using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.Enums;
using EImece.Domain.Services.IServices;
using Ninject;
using NLog;
using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    [UnderConst]
    public abstract class BaseController : Controller
    {
        [Inject]
        public ISettingService SettingService { get; set; }

        private static readonly Logger BaseLogger = LogManager.GetCurrentClassLogger();

        public void CreateLanguageCookie_OLD(EImeceLanguage selectedLanguage, string cookieName)
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

        public void CreateLanguageCookie(EImeceLanguage selectedLanguage, string cookieName)
        {
            String cultureName = EnumHelper.GetEnumDescription(selectedLanguage);
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
            //Session[Constants.CultureCookieName] = ((int)selectedLanguage) + "";
        }

        protected int CurrentLanguage
        {
            get
            {
                var lang = Thread.CurrentThread.CurrentCulture.ToString();
                return EnumHelper.GetEnumFromDescription(lang, typeof(EImeceLanguage));
            }
        }

        protected int CurrentLanguage_OLD
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

        protected override void OnException(ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.Exception != null)
            {
                BaseLogger.Error("OnException:" + filterContext.Exception.ToFormattedString());
            }
            base.OnException(filterContext);
        }
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            HttpCookie languageCookie = System.Web.HttpContext.Current.Request.Cookies["Language"];
            if (languageCookie != null)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(languageCookie.Value);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCookie.Value);
            }
            else
            {
                //other code here
            }

            base.Initialize(requestContext);
        }
    }
}