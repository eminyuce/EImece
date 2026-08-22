using EImece.Domain.Models.Enums;
using EImece.Domain.Services;
using Resources;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace EImece.Domain.Helpers.HtmlHelpers
{
    /// <summary>
    /// Renders either the Legacy arithmetic CAPTCHA or Google reCAPTCHA v2,
    /// depending on <see cref="AppConfig.CaptchaProvider"/>.
    /// </summary>
    public static class CaptchaHtmlHelper
    {
        private const string ScriptRenderedItemsKey = "EImece.Recaptcha.ScriptRendered";

        /// <summary>
        /// Renders the active captcha widget for the given Legacy Session prefix.
        /// </summary>
        /// <param name="prefix">Legacy prefix (e.g. CustomerLogin). Ignored for reCAPTCHA.</param>
        public static MvcHtmlString CaptchaWidget(
            this HtmlHelper htmlHelper,
            string prefix,
            string validationCssClass = "text-danger",
            string placeholder = null)
        {
            if (htmlHelper == null)
            {
                return MvcHtmlString.Empty;
            }

            switch (CaptchaSettings.Provider)
            {
                case CaptchaProviderType.None:
                    return MvcHtmlString.Empty;

                case CaptchaProviderType.Recaptcha:
                    return RenderRecaptcha(htmlHelper, validationCssClass);

                case CaptchaProviderType.Legacy:
                default:
                    return RenderLegacyCaptcha(htmlHelper, prefix, validationCssClass, placeholder);
            }
        }

        /// <summary>
        /// Renders reCAPTCHA v2 only when CaptchaProvider is Recaptcha; otherwise empty.
        /// Prefer <see cref="CaptchaWidget"/> for dual-mode forms.
        /// </summary>
        public static MvcHtmlString Recaptcha(this HtmlHelper htmlHelper, string validationCssClass = "text-danger")
        {
            if (htmlHelper == null || CaptchaSettings.Provider != CaptchaProviderType.Recaptcha)
            {
                return MvcHtmlString.Empty;
            }

            return RenderRecaptcha(htmlHelper, validationCssClass);
        }

        private static MvcHtmlString RenderRecaptcha(HtmlHelper htmlHelper, string validationCssClass)
        {
            var siteKey = CaptchaSettings.RecaptchaSiteKey;
            if (string.IsNullOrWhiteSpace(siteKey))
            {
                return MvcHtmlString.Create(
                    "<div class=\"alert alert-warning\">reCAPTCHA site key is not configured.</div>");
            }

            var sb = new StringBuilder();
            sb.Append("<div class=\"form-group recaptcha-container mb-3\">");
            sb.AppendFormat(
                "<div class=\"g-recaptcha\" data-sitekey=\"{0}\"></div>",
                HttpUtility.HtmlAttributeEncode(siteKey));
            sb.Append(htmlHelper.ValidationMessage(CaptchaService.ModelStateKey, new { @class = validationCssClass }));
            sb.Append(htmlHelper.ValidationMessage(RecaptchaService.ModelStateKey, new { @class = validationCssClass }));
            sb.Append("</div>");

            var httpContext = htmlHelper.ViewContext.HttpContext;
            if (httpContext != null && httpContext.Items[ScriptRenderedItemsKey] == null)
            {
                httpContext.Items[ScriptRenderedItemsKey] = true;
                sb.Append("<script src=\"https://www.google.com/recaptcha/api.js\" async defer></script>");
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        private static MvcHtmlString RenderLegacyCaptcha(
            HtmlHelper htmlHelper,
            string prefix,
            string validationCssClass,
            string placeholder)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var imageUrl = urlHelper.Action("GetCaptcha", "Images", new RouteValueDictionary
            {
                { "prefix", prefix }
            });

            var label = Resource.AnswerSecurityQuestion;
            try
            {
                label = Resource.ContactUsCaptcha;
            }
            catch
            {
                // fall back to AnswerSecurityQuestion
            }

            var inputPlaceholder = placeholder ?? Resource.AnswerSecurityQuestion;
            var requiredMessage = Resource.AnswerSecurityQuestion;

            var value = htmlHelper.ViewData.Model != null
                ? (htmlHelper.ViewData.Eval("Captcha") as string) ?? string.Empty
                : string.Empty;

            var sb = new StringBuilder();
            sb.Append("<div class=\"form-group captcha-container mb-3\">");
            sb.AppendFormat("<label for=\"Captcha\">{0} <span class=\"text-danger\" aria-hidden=\"true\">*</span></label>", HttpUtility.HtmlEncode(label));
            sb.AppendFormat(
                "<div class=\"mb-2\"><img width=\"180\" height=\"50\" rel=\"nofollow\" src=\"{0}\" alt=\"Captcha\" class=\"captcha-img\" /></div>",
                HttpUtility.HtmlAttributeEncode(imageUrl));
            sb.AppendFormat(
                "<input type=\"text\" name=\"Captcha\" id=\"Captcha\" value=\"{0}\" class=\"form-control\" autocomplete=\"off\" inputmode=\"numeric\" maxlength=\"2\" required=\"required\" aria-required=\"true\" data-val=\"true\" data-val-required=\"{1}\" data-review-field=\"captcha\" placeholder=\"{2}\" />",
                HttpUtility.HtmlAttributeEncode(value),
                HttpUtility.HtmlAttributeEncode(requiredMessage),
                HttpUtility.HtmlAttributeEncode(inputPlaceholder));
            sb.Append(htmlHelper.ValidationMessage(CaptchaService.ModelStateKey, new { @class = validationCssClass }));
            sb.Append("</div>");

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}
