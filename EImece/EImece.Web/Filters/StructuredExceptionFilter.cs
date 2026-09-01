using EImece.Domain.Observability.Configuration;
using EImece.Domain.Observability.Exceptions;
using EImece.Domain.Observability.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.Mvc;

namespace EImece.Web.Filters
{
    public sealed class StructuredExceptionFilter : IExceptionFilter
    {
        private readonly ObservabilityOptions _options;
        private readonly ILogger<StructuredExceptionFilter> _logger;

        public StructuredExceptionFilter(ObservabilityOptions options, ILogger<StructuredExceptionFilter> logger)
        {
            _options = options ?? ObservabilityOptions.FromAppConfig();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
