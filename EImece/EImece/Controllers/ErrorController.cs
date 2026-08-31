using EImece.Web.Controllers;
using EImece.Domain;
using EImece.Web.Filters;
using EImece.Domain.Models.HelperModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Net;
using System.Web.Mvc;
using EImece.Domain.Services.IServices;

namespace EImece.Controllers
{
    /// <summary>
    /// Provides methods that respond to HTTP requests with HTTP errors.
    /// </summary>
    public class ErrorController : BaseController
    {
        public ErrorController()
            : this(null, null, NullLogger<ErrorController>.Instance)
        {
        }

        public ErrorController(ISettingService settingService, AutoMapper.IMapper mapper, ILogger<ErrorController> logger)
            : base(settingService, mapper, logger ?? NullLogger<ErrorController>.Instance)
        {
        }

        #region Public Methods

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Returns a HTTP 400 Bad Request error view. Returns a partial view if the request is an AJAX call.
        /// </summary>
        /// <returns>The partial or full bad request view.</returns>
        [CustomOutputCache(CacheProfile = "BadRequest")]
        public ActionResult BadRequest()
        {
            return this.GetErrorView(HttpStatusCode.BadRequest, "badrequest");
        }

        /// <summary>
        /// Returns a HTTP 403 Forbidden error view. Returns a partial view if the request is an AJAX call.
        /// Unlike a 401 Unauthorized response, authenticating will make no difference.
        /// </summary>
        /// <returns>The partial or full forbidden view.</returns>
        [CustomOutputCache(CacheProfile = "Forbidden")]
        public ActionResult Forbidden()
        {
            return this.GetErrorView(HttpStatusCode.Forbidden, "forbidden");
        }

        /// <summary>
        /// Returns a HTTP 500 Internal Server Error error view. Returns a partial view if the request is an AJAX call.
        /// </summary>
        /// <returns>The partial or full internal server error view.</returns>
        public ActionResult InternalServerError()
        {
            if (HttpContext.IsDebuggingEnabled)
            {
                ViewBag.ExceptionDetail = TempData["LastException"] as Exception ?? Server.GetLastError();
            }
            return this.GetErrorView(HttpStatusCode.InternalServerError, "internalservererror");
        }

        /// <summary>
        /// Returns a HTTP 405 Method Not Allowed error view. Returns a partial view if the request is an AJAX call.
        /// </summary>
        /// <returns>The partial or full method not allowed view.</returns>
        [CustomOutputCache(CacheProfile = "MethodNotAllowed")]
        public ActionResult MethodNotAllowed()
        {
            return this.GetErrorView(HttpStatusCode.MethodNotAllowed, "methodnotallowed");
        }

        /// <summary>
        /// Returns a HTTP 404 Not Found error view. Returns a partial view if the request is an AJAX call.
        /// </summary>
        /// <returns>The partial or full not found view.</returns>
        public ActionResult NotFound()
        {
            return this.GetErrorView(HttpStatusCode.NotFound, "notfound");
        }

        /// <summary>
        /// Returns a HTTP 401 Unauthorized error view. Returns a partial view if the request is an AJAX call.
        /// </summary>
        /// <returns>The partial or full unauthorized view.</returns>
        [CustomOutputCache(CacheProfile = "Unauthorized")]
        public ActionResult Unauthorized()
        {
            return this.GetErrorView(HttpStatusCode.Unauthorized, "unauthorized");
        }

        /// <summary>
        /// Returns a HTTP 410 Gone error view for deleted or deactivated entities. Returns a partial view if the request is an AJAX call.
        /// </summary>
        /// <returns>The partial or full gone view.</returns>
        [CustomOutputCache(CacheProfile = "NotFound")]
        public ActionResult Gone()
        {
            return this.GetErrorView((HttpStatusCode)410, "gone");
        }

        #endregion Public Methods

        #region Private Methods

        private ActionResult GetErrorView(HttpStatusCode statusCode, string viewName)
        {
            this.Response.StatusCode = (int)statusCode;
            this.Response.TrySkipIisCustomErrors = true;
            if (this.Server != null)
            {
                this.Server.ClearError();
            }

            ErrorModel error = new ErrorModel()
            {
                RequestedUrl = this.Request.Url.ToString(),
                ReferrerUrl = (this.Request.UrlReferrer == null) ? null : this.Request.UrlReferrer.ToString()
            };

            Logger.LogDebug("HTTP {0} {1}. Url={2} Referrer={3}",
                (int)statusCode,
                viewName,
                error.RequestedUrl,
                error.ReferrerUrl);

            if (this.Request.IsAjaxRequest())
            {
                return this.PartialView(viewName, error);
            }

            return this.View(viewName, error);
        }

        #endregion Private Methods
    }
}
