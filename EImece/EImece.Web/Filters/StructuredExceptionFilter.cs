using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Exceptions;
using EImece.Domain.Observability.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EImece.Web.Filters
{
    public sealed class StructuredExceptionFilter : IExceptionFilter
    {
        private readonly ObservabilityOptions _options;

        public StructuredExceptionFilter()
        {
            _options = ObservabilityOptions.FromAppConfig();
        }

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.Request.IsAjaxRequest())
            {
                return;
            }

            StructuredLoggingBootstrap.LogException(filterContext.Exception, "Unhandled AJAX exception");

            var statusCode = filterContext.Exception is HttpException httpException
                ? httpException.GetHttpCode()
                : 500;

            var response = ApiErrorResponse.Create(
                statusCode,
                "An unexpected error occurred.",
                CorrelationIdContext.Ensure(),
                filterContext.HttpContext.IsDebuggingEnabled ? filterContext.Exception.Message : null);

            filterContext.Result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(response),
                ContentType = "application/json"
            };
            filterContext.HttpContext.Response.StatusCode = statusCode;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            filterContext.ExceptionHandled = true;
        }
    }
}
