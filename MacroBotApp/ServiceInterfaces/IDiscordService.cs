using MacroBot.Models.Webhook;

namespace MacroBot.ServiceInterfaces;

public interface IDiscordService
{
	public bool DiscordReady { get; }
	public Task BroadcastWebhookAsync(WebhookItem webhook, WebhookRequest webhookRequest);
}