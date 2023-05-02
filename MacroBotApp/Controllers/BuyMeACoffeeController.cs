using MacroBot.Authentication;
using MacroBot.Config;
using MacroBot.Models.BuyMeACoffee;
using MacroBot.Models.Webhook;
using MacroBot.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;

namespace MacroBot.Controllers;

[ApiController]
[Route("/buymeaocoffee")]
public class BuyMeACoffeeController : ControllerBase
{
    private readonly IDiscordService _discordService;
    private readonly BuyMeACoffeeConfig _buyMeACoffeeConfig;

    public BuyMeACoffeeController(IDiscordService discordService, BuyMeACoffeeConfig buyMeACoffeeConfig)
    {
        _discordService = discordService;
        _buyMeACoffeeConfig = buyMeACoffeeConfig;
    }

    [HttpPost("donationcreated")]
    [BuyMeACoffeeWebhook]
    public async Task<IActionResult> DonationCreated([FromBody] BuyMeACoffeeDonationCreatedRequest donationCreatedRequest)
    {
        var supporterName = donationCreatedRequest.Data?.SupporterName ?? "Anonymous";
        var amount = donationCreatedRequest.Data?.Amount?.ToString("#0.00") ?? "Hidden";
        var currency = donationCreatedRequest.Data?.Currency ?? string.Empty;
        var message = donationCreatedRequest.Data?.NoteHidden == "true"
            ? donationCreatedRequest.Data?.SupportNote
            : null;

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

        var webhook = new WebhookItem
        {
            Id = "BuyMeACoffee",
            ChannelId = _buyMeACoffeeConfig.ChannelId
        };

        await _discordService.BroadcastWebhookAsync(webhook, webHookRequest);
        return Ok();
    }
}