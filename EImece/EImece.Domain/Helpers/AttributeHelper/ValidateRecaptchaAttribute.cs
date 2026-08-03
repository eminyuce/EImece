using System;
using System.Web.Mvc;

namespace EImece.Domain.Helpers.AttributeHelper
{
    /// <summary>
    /// Backward-compatible alias. Prefer <see cref="ValidateCaptchaAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateRecaptchaAttribute : ActionFilterAttribute
    {
        public string Prefix { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var inner = new ValidateCaptchaAttribute(Prefix);
            inner.OnActionExecuting(filterContext);
        }
    }
}
