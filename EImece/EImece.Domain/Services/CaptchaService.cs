using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using NLog;
using Resources;
using System;
using System.Web;
using System.Web.Mvc;

namespace EImece.Domain.Services
{
    /// <summary>
    /// Unified captcha validation for Legacy (arithmetic) and Google reCAPTCHA modes.
    /// </summary>
    public static class CaptchaService
    {
        public const string ModelStateKey = "Captcha";
        public const string FormFieldName = "Captcha";
        public const string SessionKeyPrefix = "Captcha";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static bool HasValidationError(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                return false;
            }

            if (modelState.ContainsKey(ModelStateKey)
                && modelState[ModelStateKey] != null
                && modelState[ModelStateKey].Errors.Count > 0)
            {
                return true;
            }

            // reCAPTCHA mode may still use the Recaptcha ModelState key
            return RecaptchaService.HasValidationError(modelState);
        }

        public static string GetErrorMessage()
        {
            try
            {
                if (CaptchaSettings.Provider == CaptchaProviderType.Recaptcha)
                {
                    return Resource.RecaptchaValidationFailed;
                }

                return Resource.WrongSum;
            }
            catch
            {
                return "Captcha validation failed. Please try again.";
            }
        }

        public static string GetSessionKey(string prefix)
        {
            return SessionKeyPrefix + (prefix ?? string.Empty);
        }

        /// <summary>
        /// Validates the current request according to <see cref="CaptchaSettings.Provider"/>.
        /// </summary>
        public static bool ValidateRequest(HttpContextBase httpContext, string legacyPrefix)
        {
            if (httpContext == null)
            {
                return false;
            }

            switch (CaptchaSettings.Provider)
            {
                case CaptchaProviderType.None:
                    Logger.Debug("Captcha validation skipped (CaptchaProvider=None).");
                    return true;

                case CaptchaProviderType.Recaptcha:
                    return RecaptchaService.ValidateRequest(httpContext.Request);

                case CaptchaProviderType.Legacy:
                default:
                    return ValidateLegacy(httpContext, legacyPrefix);
            }
        }

        public static bool ValidateLegacy(HttpContextBase httpContext, string prefix)
        {
            if (httpContext?.Session == null || httpContext.Request == null)
            {
                Logger.Error("Legacy captcha validation failed: Session or Request is null.");
                return false;
            }

            var sessionKey = GetSessionKey(prefix);
            var expected = httpContext.Session[sessionKey];
            var submitted = httpContext.Request.Form[FormFieldName];

            if (expected == null || string.IsNullOrWhiteSpace(submitted))
            {
                Logger.Warn($"Legacy captcha validation failed. SessionKey={sessionKey}, hasSession={(expected != null)}, hasInput={!string.IsNullOrWhiteSpace(submitted)}");
                return false;
            }

            var isValid = expected.ToString().Equals(submitted.Trim(), StringComparison.InvariantCultureIgnoreCase);
            if (!isValid)
            {
                Logger.Warn($"Legacy captcha mismatch. SessionKey={sessionKey}, Expected={expected}, Input={submitted}");
            }

            return isValid;
        }
    }
}
