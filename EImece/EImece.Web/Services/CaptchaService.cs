using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Observability.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Resources;
using System;
using System.Web;
using System.Web.Mvc;

namespace EImece.Web.Services
{
    /// <summary>
    /// Unified captcha validation for Legacy (arithmetic) and Google reCAPTCHA modes.
    /// </summary>
    public static class CaptchaService
    {
        public const string ModelStateKey = "Captcha";
        public const string FormFieldName = "Captcha";
        public const string SessionKeyPrefix = "Captcha";

        private static ILogger Logger =>
            LoggingBootstrap.LoggerFactory?.CreateLogger(typeof(CaptchaService))
            ?? NullLogger.Instance;

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

        public static bool ValidateRequest(HttpContextBase httpContext, string legacyPrefix)
        {
            if (httpContext == null)
            {
                return false;
            }

            switch (CaptchaSettings.Provider)
            {
                case CaptchaProviderType.None:
                    Logger.LogDebug("Captcha validation skipped (CaptchaProvider=None).");
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
                Logger.LogError("Legacy captcha validation failed: Session or Request is null.");
                return false;
            }

            var sessionKey = GetSessionKey(prefix);
            var expected = httpContext.Session[sessionKey];
            var submitted = httpContext.Request.Form[FormFieldName];

            if (expected == null || string.IsNullOrWhiteSpace(submitted))
            {
                Logger.LogWarning(
                    "Legacy captcha validation failed. SessionKey={SessionKey} HasSession={HasSession} HasInput={HasInput}",
                    sessionKey,
                    expected != null,
                    !string.IsNullOrWhiteSpace(submitted));
                return false;
            }

            var isValid = expected.ToString().Equals(submitted.Trim(), StringComparison.InvariantCultureIgnoreCase);
            if (!isValid)
            {
                Logger.LogWarning("Legacy captcha mismatch. SessionKey={SessionKey}", sessionKey);
            }

            return isValid;
        }
    }
}
