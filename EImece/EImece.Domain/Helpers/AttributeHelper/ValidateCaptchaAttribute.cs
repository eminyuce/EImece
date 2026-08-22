using EImece.Domain.Models.Enums;
using EImece.Domain.Services;
using System;
using System.Web.Mvc;

namespace EImece.Domain.Helpers.AttributeHelper
{
    /// <summary>
    /// Validates captcha on POST actions according to <see cref="AppConfig.CaptchaProvider"/>.
    /// Use <see cref="Prefix"/> for Legacy (arithmetic) Session key: Captcha{Prefix}.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateCaptchaAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Legacy Session prefix (e.g. AdminLogin → Session["CaptchaAdminLogin"]).
        /// Ignored when CaptchaProvider is Recaptcha or None.
        /// </summary>
        public string Prefix { get; set; }

        public ValidateCaptchaAttribute()
        {
        }

        public ValidateCaptchaAttribute(string prefix)
        {
            Prefix = prefix;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            var httpMethod = filterContext.HttpContext?.Request?.HttpMethod;
            if (!string.Equals(httpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            if (CaptchaSettings.Provider == CaptchaProviderType.None)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var isValid = CaptchaService.ValidateRequest(filterContext.HttpContext, Prefix);
            if (!isValid)
            {
                filterContext.Controller.ViewData.ModelState.AddModelError(
                    CaptchaService.ModelStateKey,
                    CaptchaService.GetErrorMessage());
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
