using EImece.Domain.Entities;
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

        public void CreateLanguageCookie(EImeceLanguage selectedLanguage, string cookieName)
        {
            String cultureName = EnumHelper.GetEnumDescription(selectedLanguage);
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
        }

        protected int CurrentLanguage
        {
            get
            {
                var lang = Thread.CurrentThread.CurrentCulture.ToString();
                return EnumHelper.GetEnumFromDescription(lang, typeof(EImeceLanguage));
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
                SetCurrentCulture(languageCookie.Value);
            }
            else
            {
                //other code here
            }

            base.Initialize(requestContext);
        }
        protected void SetCurrentCulture(BaseEntity entity)
        {
            if (entity == null)
                return;
            SetCurrentCulture(entity.Lang);
        }
        protected void SetCurrentCulture(int language)
        {
            EImeceLanguage eImeceLanguage =  (EImeceLanguage) language;
            SetCurrentCulture(EnumHelper.GetEnumDescription(eImeceLanguage));
        }
        protected void SetCurrentCulture(String language)
        {
            if (String.IsNullOrEmpty(language))
                return;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
        }
        protected void SetLanguage(string id)
        {
            EImeceLanguage selectedLanguage = (EImeceLanguage)id.ToInt();
            String cultureName = EnumHelper.GetEnumDescription(selectedLanguage);
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);

            CreateLanguageCookie(selectedLanguage, Constants.CultureCookieName);
            MemoryCacheProvider.ClearAll();

            Response.Cookies.Remove("Language");

            var languageCookie = System.Web.HttpContext.Current.Request.Cookies["Language"];

            if (languageCookie == null) languageCookie = new HttpCookie("Language");

            languageCookie.Value = cultureName;

            languageCookie.Expires = DateTime.Now.AddDays(10);

            Response.SetCookie(languageCookie);
        }
    }
}