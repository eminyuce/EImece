namespace EImece.Domain.Core.Email;

public interface IEmailTemplateRenderer
{
    /// <summary>Renders a Fluid/Liquid template string with the given model.</summary>
    ValueTask<string> RenderAsync(string template, object model, CancellationToken cancellationToken = default);
}
