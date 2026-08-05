using EImece.Domain.Core.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EImece.Web.Controllers;

/// <summary>
/// Debug-friendly integration probes (Phase 8). Safe in Development; no secrets returned.
/// </summary>
[ApiController]
[Route("api/integrations")]
public sealed class IntegrationsController : ControllerBase
{
    private readonly IEmailSender _email;
    private readonly IEmailTemplateRenderer _templates;
    private readonly IHostEnvironment _env;

    public IntegrationsController(
        IEmailSender email,
        IEmailTemplateRenderer templates,
        IHostEnvironment env)
    {
        _email = email;
        _templates = templates;
        _env = env;
    }

    [HttpPost("email/test")]
    [AllowAnonymous]
    public async Task<IActionResult> SendTestEmail([FromQuery] string? to, CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment())
        {
            return NotFound();
        }

        var destination = string.IsNullOrWhiteSpace(to) ? "demo@eimece.local" : to.Trim();
        var body = await _templates.RenderAsync(
            "<h1>EImece</h1><p>Phase 8 MailKit test at {{ Timestamp }}.</p>",
            new { Timestamp = DateTime.UtcNow.ToString("o") },
            cancellationToken).ConfigureAwait(false);

        var result = await _email.SendAsync(new EmailMessage
        {
            ToAddress = destination,
            Subject = "EImece Phase 8 email test",
            HtmlBody = body
        }, cancellationToken).ConfigureAwait(false);

        return Ok(new
        {
            result.Sent,
            result.LoggedOnly,
            result.Error,
            To = destination
        });
    }
}
