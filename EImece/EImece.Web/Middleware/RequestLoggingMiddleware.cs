using System.Diagnostics;
using EImece.Domain.Core.Configuration;
using Microsoft.Extensions.Options;

namespace EImece.Web.Middleware;

/// <summary>
/// Lightweight request logging (legacy RequestLoggingActionFilter parity).
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<ObservabilityOptions> observability)
    {
        if (!observability.Value.EnableRequestLogging)
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        finally
        {
            sw.Stop();
            var correlationId = context.Items[CorrelationIdMiddleware.ItemKey] as string;
            _logger.LogInformation(
                "HTTP {Method} {Path} => {StatusCode} in {ElapsedMs}ms (CorrelationId={CorrelationId})",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId);
        }
    }
}
