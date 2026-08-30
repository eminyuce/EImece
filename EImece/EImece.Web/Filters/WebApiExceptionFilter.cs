using EImece.Domain.Observability.Logging;
using System.Web.Http.Filters;

namespace EImece.Web.Filters
{
    public sealed class WebApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception == null)
            {
                return;
            }

            StructuredLoggingBootstrap.LogException(actionExecutedContext.Exception, "Unhandled Web API exception");
        }
    }
}
