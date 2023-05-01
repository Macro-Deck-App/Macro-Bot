using MacroBot.Config;
using MacroBot.Models.BuyMeACoffee;
using MacroBot.Models.Webhook;
using MacroBot.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace MacroBot.Controllers;

[ApiController]
[Route("/buymeacoffee")]
public class BuyMeACoffeeController : ControllerBase
{
    private readonly IDiscordService _discordService;
    private readonly WebhooksConfig _webhooksConfig;

    public BuyMeACoffeeController(IDiscordService discordService, WebhooksConfig webhooksConfig)
    {
        _discordService = discordService;
        _webhooksConfig = webhooksConfig;
    }

    [HttpPost("donationcreated")]
    public async Task<IActionResult> DonationCreated([FromQuery] BuyMeACoffeeDonationCreatedRequest donationCreatedRequest)
    {
        if (donationCreatedRequest.Data?.Succeeded is false)
        {
            return BadRequest();
        }
        var webhook = _webhooksConfig.Webhooks.FirstOrDefault(x => x.Id.Equals("buymeacoffee"));
        if (webhook == null)
        {
            return NotFound();
        }
        
        var supporterName = donationCreatedRequest.Data?.SupporterName ?? "anonymous";
        var amount = donationCreatedRequest.Data?.Amount?.ToString("#0.00") ?? "hidden";
        var currency = donationCreatedRequest.Data?.Currency ?? string.Empty;
        var message = donationCreatedRequest.Data?.NoteHidden == true
            ? donationCreatedRequest.Data?.SupportNote
            : string.Empty;

        var webHookRequest = new WebhookRequest
        {
            Embed = new WebhookRequestEmbed
            {
                Color = new WebhookRequestEmbedColor
                {
                    R = 0,
                    G = 1,
                    B = 0
                },
                Title = "New donation",
                Fields = new List<WebhookRequestEmbedField>()
                {
                    new ()
                    {
                        Name = "Donator",
                        Value = supporterName,
                        Inline = true
                    },
                    new ()
                    {
                        Name = "Amount",
                        Value = $"{amount} {currency}",
                        Inline = true
                    }
                }
            }
        };

        if (!string.IsNullOrWhiteSpace(message))
        {
            webHookRequest.Embed.Fields.Add(new WebhookRequestEmbedField
            {
                Name = "Message",
                Value = message,
                Inline = false
            });
        }
        
        webHookRequest.Embed.Fields.Add(new WebhookRequestEmbedField
        {
            Name = "\nAlso want to donate to the project?",
            Value = "https://buymeacoffee.com/suchbyte",
            Inline = false
        });

        await _discordService.BroadcastWebhookAsync(webhook, webHookRequest);
        return Ok();
    }
}