using MacroBot.Authentication;
using MacroBot.Config;
using MacroBot.Models.Webhook;
using MacroBot.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroBot.Controllers;

[ApiController]
[Route("/webhook")]
public class WebhookController : ControllerBase
{
    private readonly IDiscordService _discordService;
    private readonly ILogger _logger = Log.ForContext<WebhookController>();

    private readonly WebhooksConfig _webhooksConfig;

    public WebhookController(WebhooksConfig webhooksConfig,
        IDiscordService discordService)
    {
        _webhooksConfig = webhooksConfig;
        _discordService = discordService;
    }

    [HttpPost("{webhookId}")]
    public async Task<IActionResult> RunAsync(string webhookId,
        [FromBody] WebhookRequest webhookRequest)
    {
        var webhook = _webhooksConfig.Webhooks.FirstOrDefault(x => x.Id.Equals(webhookId));
        if (webhook is null) return NotFound();

        var authResult = Request.CheckAuthentication(webhook);

        switch (authResult)
        {
            case AuthenticationResult.Unauthorized:
                return StatusCode(StatusCodes.Status401Unauthorized);
            case AuthenticationResult.Forbidden:
                return StatusCode(StatusCodes.Status403Forbidden);
            case AuthenticationResult.Authorized:
            default:
                await _discordService.BroadcastWebhookAsync(webhook, webhookRequest);
                return Ok();
        }
    }
}