using EImece.Domain;
using EImece.Domain.Helpers;
using NLog;
using System;
using System.Net;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;

namespace EImece.Web.Filters
{
    /// <summary>
    /// Action filter attribute providing in-memory sliding-window rate limiting on sensitive public endpoints.
    /// Configurable via Web.config appSettings keys: RateLimit:{Feature}:Limit and RateLimit:{Feature}:WindowMinutes.
    /// Master toggle: RateLimit:Enabled (default: true).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Feature partition identifier (e.g. "login", "contact", "checkout", "search").
        /// </summary>
        public string FeatureKey { get; }

        /// <summary>
        /// Default request limit if not configured in Web.config.
        /// </summary>
        public int DefaultLimit { get; set; }

        /// <summary>
        /// Default sliding window duration in minutes if not configured in Web.config.
        /// </summary>
        public int DefaultWindowMinutes { get; set; }

        public RateLimitAttribute(string featureKey, int defaultLimit = 10, int defaultWindowMinutes = 1)
        {
            FeatureKey = featureKey ?? throw new ArgumentNullException(nameof(featureKey));
            DefaultLimit = defaultLimit;
            DefaultWindowMinutes = defaultWindowMinutes;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null) return;

            // 1. Check if rate limiting is globally enabled (e.g. disable for local testing)
            var settingService = DependencyResolver.Current?.GetService<EImece.Domain.Services.IServices.ISettingService>();
            bool isEnabled = settingService != null
                ? settingService.GetSettingByKey(Constants.RateLimit_Enabled).ToBool(true)
                : true;

            if (!isEnabled)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var request = filterContext.HttpContext.Request;

            // 2. Bypass rate limiting for authenticated Admins on admin routes
            if (filterContext.HttpContext.User != null
                && filterContext.HttpContext.User.Identity != null
                && filterContext.HttpContext.User.Identity.IsAuthenticated
                && filterContext.HttpContext.User.IsInRole(Domain.Constants.AdministratorRole))
            {
                var area = filterContext.RouteData.DataTokens["area"] as string;
                if (string.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    base.OnActionExecuting(filterContext);
                    return;
                }
            }

            // 3. Resolve Client IP safely (handling X-Forwarded-For)
            string clientIp = ResolveClientIp(request);

            // 4. Resolve Limit and Window from database or Attribute Defaults
            var limitStr = settingService?.GetSettingByKey($"RateLimit:{FeatureKey}:Limit");
            int limit = !string.IsNullOrWhiteSpace(limitStr) ? limitStr.ToInt(DefaultLimit) : DefaultLimit;

            var windowStr = settingService?.GetSettingByKey($"RateLimit:{FeatureKey}:WindowMinutes");
            int windowMinutes = !string.IsNullOrWhiteSpace(windowStr) ? windowStr.ToInt(DefaultWindowMinutes) : DefaultWindowMinutes;
            if (windowMinutes <= 0) windowMinutes = 1;
            var window = TimeSpan.FromMinutes(windowMinutes);

            // 5. Build unique rate limit key (partitioned by feature and IP)
            string rateLimitKey = $"{FeatureKey.ToLowerInvariant()}:{clientIp}";

            // 6. Check limit against in-memory sliding window
            var checkResult = InMemoryRateLimiter.Check(rateLimitKey, limit, window);

            if (!checkResult.IsAllowed)
            {
                Logger.Warn($"Rate limit exceeded for feature '{FeatureKey}' from IP '{clientIp}'. Window: {windowMinutes}m, Limit: {limit}. Retry-After: {checkResult.RetryAfterSeconds}s.");

                filterContext.HttpContext.Response.StatusCode = 429;
                filterContext.HttpContext.Response.Headers["Retry-After"] = checkResult.RetryAfterSeconds.ToString();

                string userMessage = GetFriendlyErrorMessage(FeatureKey);

                if (request.IsAjaxRequest() || (request.ContentType != null && request.ContentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            status = "error",
                            message = userMessage,
                            retryAfter = checkResult.RetryAfterSeconds
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    filterContext.Controller.ViewData.ModelState.AddModelError("", userMessage);
                    filterContext.Result = HandleExceededViewResult(filterContext, FeatureKey, userMessage, checkResult.RetryAfterSeconds);
                }

                return;
            }

            base.OnActionExecuting(filterContext);
        }

        private static string ResolveClientIp(HttpRequestBase request)
        {
            if (request == null) return "unknown";

            string forwardedFor = request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                // Format: client, proxy1, proxy2. First element is the client IP.
                var firstIp = forwardedFor.Split(',')[0].Trim();
                if (!string.IsNullOrWhiteSpace(firstIp))
                {
                    return firstIp;
                }
            }

            return request.UserHostAddress ?? "unknown";
        }

        private static string GetFriendlyErrorMessage(string feature)
        {
            switch (feature.ToLowerInvariant())
            {
                case "login":
                    return "Çok fazla giriş denemesi yaptınız. Lütfen daha sonra tekrar deneyiniz.";
                case "contact":
                    return "İletişim formu için çok fazla istek gönderildi. Lütfen bir süre sonra tekrar deneyiniz.";
                case "checkout":
                    return "Çok fazla işlem denemesi yapıldı. Güvenliğiniz için lütfen birkaç dakika sonra tekrar deneyiniz.";
                case "search":
                    return "Çok fazla arama isteği gönderildi. Lütfen bir süre sonra tekrar deneyiniz.";
                default:
                    return "Çok fazla istek gönderildi. Lütfen daha sonra tekrar deneyiniz.";
            }
        }

        private static ActionResult HandleExceededViewResult(ActionExecutingContext filterContext, string feature, string message, int retryAfterSeconds)
        {
            // For search endpoints, return a graceful message container
            if (string.Equals(feature, "search", StringComparison.OrdinalIgnoreCase))
            {
                return new ContentResult
                {
                    Content = $"<div class='alert alert-warning text-center' style='margin:20px;'>{message}</div>",
                    ContentType = MediaTypeNames.Text.Html
                };
            }

            return new ContentResult
            {
                Content = $"<!DOCTYPE html><html><head><title>429 Too Many Requests</title><meta charset='utf-8'/><style>body{{font-family:sans-serif;text-align:center;padding:50px;background:#f8f9fa;color:#333;}} .card{{max-width:500px;margin:0 auto;background:#fff;padding:30px;border-radius:8px;box-shadow:0 2px 10px rgba(0,0,0,0.1);}} a{{display:inline-block;margin-top:15px;color:#007bff;text-decoration:none;}}</style></head><body><div class='card'><h2>İstek Sınırı Aşıldı</h2><p>{message}</p><p><small>Lütfen {retryAfterSeconds} saniye sonra tekrar deneyiniz.</small></p><a href='/'>Ana Sayfaya Dön</a></div></body></html>",
                ContentType = MediaTypeNames.Text.Html
            };
        }
    }
}
