using MacroBot.Config;
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

    public WebhookController(WebhooksConfig webhooksConfig)
    {
        _webhooksConfig = webhooksConfig;
    }
    
    [HttpGet("{webhookId}")]
    public IActionResult RunAsync(string webhookId)
    {
        var webhook = _webhooksConfig.Items.FirstOrDefault(x => x.Id.Equals(webhookId));
        if (webhook is null)
        {
            return NotFound();
        }
        
        return Ok();
    }
}