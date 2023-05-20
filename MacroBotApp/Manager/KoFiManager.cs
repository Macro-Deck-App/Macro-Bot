using System.ComponentModel;
using MacroBot.Config;
using MacroBot.Enums;
using MacroBot.ManagerInterfaces;
using MacroBot.Models.KoFi;
using MacroBot.Models.Webhook;
using MacroBot.ServiceInterfaces;

namespace MacroBot.Manager;

public class KoFiManager : IKoFiManager
{
	private readonly KoFiConfig _koFiConfig;
	private readonly IDiscordService _discordService;

	public KoFiManager(KoFiConfig koFiConfig, IDiscordService discordService)
	{
		_koFiConfig = koFiConfig;
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
			ChannelId = _koFiConfig.ChannelId
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