using System.ComponentModel;
using MacroBot.Core.Config;
using MacroBot.Core.Enums;
using MacroBot.Core.ManagerInterfaces;
using MacroBot.Core.Models.KoFi;
using MacroBot.Core.Models.Webhook;
using MacroBot.Core.ServiceInterfaces;

namespace MacroBot.Core.Manager;

public class KoFiManager : IKoFiManager
{
    private readonly IDiscordService _discordService;

    public KoFiManager(IDiscordService discordService)
    {
        _discordService = discordService;
    }

    public async Task HandleWebhook(KoFiWebhookRequest koFiWebhookRequest)
    {
        switch (koFiWebhookRequest.Type)
        {
            case KoFiWebhookType.Donation:
                await HandleDonation(koFiWebhookRequest);
                break;
            case KoFiWebhookType.Subscription:
                await HandleSubscription(koFiWebhookRequest);
                break;
            default:
                throw new InvalidEnumArgumentException("Type not found");
        }
    }

    private async Task BroadcastDiscord(WebhookRequestEmbed webhookRequestEmbed)
    {
        var webhookRequest = new WebhookRequest
        {
            Embed = webhookRequestEmbed
        };

        var webhook = new WebhookItem
        {
            Id = "Ko-Fi",
            ChannelId = MacroBotConfig.KoFiDonationChannelId
        };

        await _discordService.BroadcastWebhookAsync(webhook, webhookRequest);
    }

    private async Task HandleDonation(KoFiWebhookRequest koFiWebhookRequest)
    {
        var webhookRequestEmbed = BuildEmbed(koFiWebhookRequest);
        webhookRequestEmbed.Title = "New donation 🎊";
        await BroadcastDiscord(webhookRequestEmbed);
    }

    private async Task HandleSubscription(KoFiWebhookRequest koFiWebhookRequest)
    {
        
        var webhookRequestEmbed = BuildEmbed(koFiWebhookRequest);
        webhookRequestEmbed.Title = koFiWebhookRequest.IsFirstSubscriptionPayment 
            ? "🎉 New monthly subscription 🎉"
            : "🎉🎉 Monthly subscription renewal 🎉🎉";
        await BroadcastDiscord(webhookRequestEmbed);
    }

    private static WebhookRequestEmbed BuildEmbed(KoFiWebhookRequest koFiWebhookRequest)
    {
        var supporterName = koFiWebhookRequest.FromName ?? "Anonymous";
        var amount = koFiWebhookRequest.Amount ?? "Hidden";
        var currency = koFiWebhookRequest.Currency ?? string.Empty;
        var message = koFiWebhookRequest.IsPublic
            ? koFiWebhookRequest.Message
            : null;
        
        var webHookRequestEmbed = new WebhookRequestEmbed
        {
            Color = new WebhookRequestEmbedColor
            {
                R = 0,
                G = 1,
                B = 0
            },
            Fields = new List<WebhookRequestEmbedField>()
            {
                new ()
                {
                    Name = "From",
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
        };
        
        if (!string.IsNullOrWhiteSpace(message))
        {
            webHookRequestEmbed.Fields.Add(new WebhookRequestEmbedField
            {
                Name = "Message",
                Value = message,
                Inline = false
            });
        }
        
        webHookRequestEmbed.Fields.Add(new WebhookRequestEmbedField
        {
            Name = "Also want to support the project?",
            Value = "https://ko-fi.com/manuelmayer",
            Inline = false
        });

        return webHookRequestEmbed;
    }
}