using EImece.Domain.Observability.Logging;

namespace EImece.App_Start
{
    public static class ObservabilityBootstrap
    {
        public static void Configure()
        {
            // The resilient HTTP client is now consumed via constructor injection
            // (see IImageDownloadService); no global static accessor to prime here.
            StructuredLoggingBootstrap.Configure();
        }
    }
}
