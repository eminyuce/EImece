using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using EImece.Domain;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Observability.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace EImece.Web.Services
{
    /// <summary>
    /// Server-side Google reCAPTCHA v2 verification.
    /// </summary>
    public static class RecaptchaService
    {
        public const string ResponseFormKey = "g-recaptcha-response";
        public const string ModelStateKey = "Recaptcha";

        private static ILogger Logger =>
            LoggingBootstrap.LoggerFactory?.CreateLogger(typeof(RecaptchaService))
            ?? NullLogger.Instance;

        public static bool HasValidationError(ModelStateDictionary modelState)
        {
            if (modelState == null)
            {
                return false;
            }

            return modelState.ContainsKey(ModelStateKey)
                   && modelState[ModelStateKey] != null
                   && modelState[ModelStateKey].Errors.Count > 0;
        }

        public static bool ValidateRequest(HttpRequestBase request)
        {
            if (CaptchaSettings.Provider != CaptchaProviderType.Recaptcha)
            {
                Logger.LogDebug("reCAPTCHA validation skipped because CaptchaProvider is not Recaptcha.");
                return true;
            }

            if (request == null)
            {
                Logger.LogError("reCAPTCHA validation failed: HTTP request is null.");
                return false;
            }

            var response = request.Form[ResponseFormKey];
            if (string.IsNullOrWhiteSpace(response))
            {
                Logger.LogWarning("reCAPTCHA validation failed: missing g-recaptcha-response.");
                return false;
            }

            var remoteIp = GetClientIp(request);
            return VerifyWithGoogle(AppConfig.RecaptchaSecretKey, response, remoteIp);
        }

        public static bool VerifyWithGoogle(string secretKey, string responseToken, string remoteIp = null)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                Logger.LogError("reCAPTCHA secret key is not configured.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(responseToken))
            {
                return false;
            }

            try
            {
                EnsureTls12();

                using (var client = new WebClient())
                {
                    var values = new NameValueCollection
                    {
                        { "secret", secretKey },
                        { "response", responseToken }
                    };

                    if (!string.IsNullOrWhiteSpace(remoteIp))
                    {
                        values.Add("remoteip", remoteIp);
                    }

                    var resultBytes = client.UploadValues(AppConfig.RecaptchaSiteVerifyUrl, "POST", values);
                    var json = Encoding.UTF8.GetString(resultBytes);
                    var result = JsonConvert.DeserializeObject<RecaptchaVerifyResponse>(json);

                    if (result == null)
                    {
                        Logger.LogError("reCAPTCHA siteverify returned an empty/unreadable response.");
                        return false;
                    }

                    if (!result.Success)
                    {
                        var errors = result.ErrorCodes != null
                            ? string.Join(", ", result.ErrorCodes)
                            : "(none)";
                        Logger.LogWarning($"reCAPTCHA siteverify failed. Error codes: {errors}");
                    }

                    return result.Success;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "reCAPTCHA siteverify request failed.");
                return false;
            }
        }

        private static string GetClientIp(HttpRequestBase request)
        {
            var forwarded = request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrWhiteSpace(forwarded))
            {
                var first = forwarded.Split(',')[0].Trim();
                if (!string.IsNullOrWhiteSpace(first))
                {
                    return first;
                }
            }

            return request.UserHostAddress;
        }

        private static void EnsureTls12()
        {
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
            catch (NotSupportedException)
            {
                // Ignore if the runtime does not allow altering the protocol flags.
            }
        }

        private sealed class RecaptchaVerifyResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("challenge_ts")]
            public string ChallengeTs { get; set; }

            [JsonProperty("hostname")]
            public string Hostname { get; set; }

            [JsonProperty("error-codes")]
            public string[] ErrorCodes { get; set; }
        }
    }
}
