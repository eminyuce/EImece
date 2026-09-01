using EImece.Domain;
using EImece.Domain.Configuration;
using EImece.Domain.Helpers;
using EImece.Domain.Models.Enums;
using EImece.Domain.Observability.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

        private static IHttpClientFactory _httpClientFactory;
        private static OutboundHttpOptions _httpOptions;

        private static ILogger Logger =>
            LoggingBootstrap.LoggerFactory?.CreateLogger(typeof(RecaptchaService))
            ?? NullLogger.Instance;

        /// <summary>
        /// Called once from the composition root for static entry points that cannot use constructor DI.
        /// </summary>
        public static void Configure(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            _httpClientFactory = serviceProvider.GetService(typeof(IHttpClientFactory)) as IHttpClientFactory;
            var options = serviceProvider.GetService(typeof(IOptions<OutboundHttpOptions>)) as IOptions<OutboundHttpOptions>;
            _httpOptions = options?.Value ?? OutboundHttpOptions.FromAppConfig();
        }

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
            var secretKey = _httpOptions?.RecaptchaSecretKey;
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                secretKey = AppConfig.RecaptchaSecretKey;
            }

            return VerifyWithGoogle(secretKey, response, remoteIp);
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
                return VerifyWithGoogleAsync(secretKey, responseToken, remoteIp)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "reCAPTCHA siteverify request failed.");
                return false;
            }
        }

        private static async Task<bool> VerifyWithGoogleAsync(string secretKey, string responseToken, string remoteIp)
        {
            var verifyUrl = _httpOptions?.RecaptchaSiteVerifyUrl;
            if (string.IsNullOrWhiteSpace(verifyUrl))
            {
                verifyUrl = AppConfig.RecaptchaSiteVerifyUrl;
            }

            var form = new Dictionary<string, string>
            {
                { "secret", secretKey },
                { "response", responseToken }
            };

            if (!string.IsNullOrWhiteSpace(remoteIp))
            {
                form["remoteip"] = remoteIp;
            }

            HttpClient client = _httpClientFactory != null
                ? _httpClientFactory.CreateClient(HttpClientNames.Recaptcha)
                : new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

            try
            {
                using (var content = new FormUrlEncodedContent(form))
                using (var response = await client.PostAsync(verifyUrl, content).ConfigureAwait(false))
                {
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
                        Logger.LogWarning("reCAPTCHA siteverify failed. Error codes: {ErrorCodes}", errors);
                    }

                    return result.Success;
                }
            }
            finally
            {
                if (_httpClientFactory == null)
                {
                    client.Dispose();
                }
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
