using EImece.Domain.Helpers.AttributeHelper;
using EImece.Domain.Models.HelperModels;
using System.Net;
using System.Web.Mvc;

namespace EImece.Controllers
{
    /// <summary>
    /// Provides methods that respond to HTTP requests with HTTP errors.
    /// </summary>

    public class ErrorController : BaseController
    {
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
        [CustomOutputCache(CacheProfile = "InternalServerError")]
        public ActionResult InternalServerError()
        {
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
        [CustomOutputCache(CacheProfile = "NotFound")]
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

        #endregion Public Methods

        #region Private Methods

        private ActionResult GetErrorView(HttpStatusCode statusCode, string viewName)
        {
            this.Response.StatusCode = (int)statusCode;

            // Don't show IIS custom errors.
            this.Response.TrySkipIisCustomErrors = true;

            ErrorModel error = new ErrorModel()
            {
                RequestedUrl = this.Request.Url.ToString(),
                ReferrerUrl =
                    (this.Request.UrlReferrer == null) ?
                    null :
                    this.Request.UrlReferrer.ToString()
            };

            ActionResult result;
            if (this.Request.IsAjaxRequest())
            {
                // This allows us to show errors even in partial views.
                result = this.PartialView(viewName, error);
            }
            else
            {
                result = this.View(viewName, error);
            }

            return result;
        }

        #endregion Private Methods
    }
}