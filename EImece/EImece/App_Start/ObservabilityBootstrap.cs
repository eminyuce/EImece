using EImece.Domain.Observability.Http;
using EImece.Domain.Observability.Logging;

namespace EImece.App_Start
{
    public static class ObservabilityBootstrap
    {
        public static void Configure()
        {
            StructuredLoggingBootstrap.Configure();

            var resilientHttpClient = (IResilientHttpClient)System.Web.Mvc.DependencyResolver.Current.GetService(typeof(IResilientHttpClient));
            ResilientHttpClientAccessor.Instance = resilientHttpClient;
        }
    }
}
