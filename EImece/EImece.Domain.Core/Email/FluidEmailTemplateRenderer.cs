using Fluid;
using Microsoft.Extensions.Logging;

namespace EImece.Domain.Core.Email;

public sealed class FluidEmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly FluidParser _parser = new();
    private readonly ILogger<FluidEmailTemplateRenderer> _logger;

    public FluidEmailTemplateRenderer(ILogger<FluidEmailTemplateRenderer> logger)
    {
        _logger = logger;
    }

    public async ValueTask<string> RenderAsync(string template, object model, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        if (!_parser.TryParse(template, out var parsed, out var error))
        {
            _logger.LogError("Fluid template parse failed: {Error}", error);
            throw new InvalidOperationException($"Email template parse failed: {error}");
        }

        var context = new TemplateContext(model);
        return await parsed.RenderAsync(context).ConfigureAwait(false);
    }
}
