using EImece.Domain.Observability.Http;
using EImece.Domain.Observability.Logging;
using Serilog;

namespace EImece.App_Start
{
    public static class ObservabilityBootstrap
    {
        public static void Configure()
        {
            StructuredLoggingBootstrap.Configure();

            var resilientHttpClient = System.Web.Mvc.DependencyResolver.Current.GetService<IResilientHttpClient>();
            ResilientHttpClientAccessor.Instance = resilientHttpClient;

            Log.Information("Observability infrastructure initialized.");
        }
    }
}
