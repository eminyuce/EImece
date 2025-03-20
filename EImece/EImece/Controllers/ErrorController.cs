using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.HelperModels;
using NLog; // Added for logging
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    /// <summary>
    /// Provides methods that respond to HTTP requests with HTTP errors.
    /// </summary>
    public class ErrorController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); // Added logger instance

        #region Public Methods

        public ActionResult Index()
        {
            Logger.Info("Entering Index action.");
            Logger.Info("Returning Index view.");
            return View();
        }

        /// <summary>
        /// Returns a HTTP 400 Bad Request error view. Returns a partial view if the request is an AJAX call.
        /// </summary>
        /// <returns>The partial or full bad request view.</returns>
        [CustomOutputCache(CacheProfile = "BadRequest")]
        public ActionResult BadRequest()
        {
            Logger.Info("Entering BadRequest action.");
            var result = this.GetErrorView(HttpStatusCode.BadRequest, "badrequest");
            Logger.Info("Returning BadRequest view or partial view.");
            return result;
        }

        /// <summary>
        /// Returns a HTTP 403 Forbidden error view. Returns a partial view if the request is an AJAX call.
        /// Unlike a 401 Unauthorized response, authenticating will make no difference.
        /// </summary>
        /// <returns>The partial or full forbidden view.</returns>
        [CustomOutputCache(CacheProfile = "Forbidden")]
        public ActionResult Forbidden()
        {
            Logger.Info("Entering Forbidden action.");
            var result = this.GetErrorView(HttpStatusCode.Forbidden, "forbidden");
            Logger.Info("Returning Forbidden view or partial view.");
            return result;
        }

        /// <summary>
        /// Returns a HTTP 500 Internal Server Error error view. Returns a partial view if the request is an AJAX call.
        /// </summary>
        /// <returns>The partial or full internal server error view.</returns>
        [CustomOutputCache(CacheProfile = "InternalServerError")]
        public ActionResult InternalServerError()
        {
            Logger.Info("Entering InternalServerError action.");
            var result = this.GetErrorView(HttpStatusCode.InternalServerError, "internalservererror");
            Logger.Info("Returning InternalServerError view or partial view.");
            return result;
        }

        /// <summary>
        /// Returns a HTTP 405 Method Not Allowed error view. Returns a partial view if the request is an AJAX call.
        /// </summary>
        /// <returns>The partial or full method not allowed view.</returns>
        [CustomOutputCache(CacheProfile = "MethodNotAllowed")]
        public ActionResult MethodNotAllowed()
        {
            Logger.Info("Entering MethodNotAllowed action.");
            var result = this.GetErrorView(HttpStatusCode.MethodNotAllowed, "methodnotallowed");
            Logger.Info("Returning MethodNotAllowed view or partial view.");
            return result;
        }

        /// <summary>
        /// Returns a HTTP 404 Not Found error view. Returns a partial view if the request is an AJAX call.
        /// </summary>
        /// <returns>The partial or full not found view.</returns>
        [CustomOutputCache(CacheProfile = "NotFound")]
        public ActionResult NotFound()
        {
            Logger.Info("Entering NotFound action.");
            var result = this.GetErrorView(HttpStatusCode.NotFound, "notfound");
            Logger.Info("Returning NotFound view or partial view.");
            return result;
        }

        /// <summary>
        /// Returns a HTTP 401 Unauthorized error view. Returns a partial view if the request is an AJAX call.
        /// </summary>
        /// <returns>The partial or full unauthorized view.</returns>
        [CustomOutputCache(CacheProfile = "Unauthorized")]
        public ActionResult Unauthorized()
        {
            Logger.Info("Entering Unauthorized action.");
            var result = this.GetErrorView(HttpStatusCode.Unauthorized, "unauthorized");
            Logger.Info("Returning Unauthorized view or partial view.");
            return result;
        }

        #endregion Public Methods

        #region Private Methods

        private ActionResult GetErrorView(HttpStatusCode statusCode, string viewName)
        {
            Logger.Info($"Entering GetErrorView with statusCode: {statusCode}, viewName: '{viewName}'");

            this.Response.StatusCode = (int)statusCode;
            Logger.Info($"Set Response.StatusCode to: {(int)statusCode}");

            // Don't show IIS custom errors.
            this.Response.TrySkipIisCustomErrors = true;
            Logger.Info("Set Response.TrySkipIisCustomErrors to true.");

            ErrorModel error = new ErrorModel()
            {
                RequestedUrl = this.Request.Url.ToString(),
                ReferrerUrl = (this.Request.UrlReferrer == null) ? null : this.Request.UrlReferrer.ToString()
            };
            Logger.Info($"Created ErrorModel: RequestedUrl='{error.RequestedUrl}', ReferrerUrl='{error.ReferrerUrl}'");

            ActionResult result;
            if (this.Request.IsAjaxRequest())
            {
                Logger.Info("Request is AJAX. Returning partial view.");
                result = this.PartialView(viewName, error);
            }
            else
            {
                Logger.Info("Request is not AJAX. Returning full view.");
                result = this.View(viewName, error);
            }

            Logger.Info($"Returning result for viewName: '{viewName}' with statusCode: {statusCode}");
            return result;
        }

        #endregion Private Methods
    }
}