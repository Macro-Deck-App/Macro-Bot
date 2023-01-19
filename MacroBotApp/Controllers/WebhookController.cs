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
    private readonly ILogger _logger = Log.ForContext<WebhookController>();
    
    private readonly WebhooksConfig _webhooksConfig;
    private readonly IDiscordService _discordService;

    public WebhookController(WebhooksConfig webhooksConfig,
        IDiscordService discordService)
    {
        _webhooksConfig = webhooksConfig;
        _discordService = discordService;
    }
    
    [HttpPost("{webhookId}")]
    public IActionResult RunAsync(string webhookId, 
        [FromBody] WebhookRequest webhookRequest)
    {
        var webhook = _webhooksConfig.Webhooks.FirstOrDefault(x => x.Id.Equals(webhookId));
        if (webhook is null)
        {
            return NotFound();
        }

        _discordService.BroadcastWebhookAsync(webhook, webhookRequest);
        
        return Ok();
    }
}