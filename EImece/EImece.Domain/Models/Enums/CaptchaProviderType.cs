namespace EImece.Domain.Models.Enums
{
    /// <summary>
    /// Captcha implementation selected via Web.config <c>CaptchaProvider</c>.
    /// </summary>
    public enum CaptchaProviderType
    {
        /// <summary>Original arithmetic image CAPTCHA (Session-based).</summary>
        Legacy = 0,

        /// <summary>Google reCAPTCHA v2 checkbox.</summary>
        Recaptcha = 1,

        /// <summary>No CAPTCHA validation or widget.</summary>
        None = 2
    }
}
