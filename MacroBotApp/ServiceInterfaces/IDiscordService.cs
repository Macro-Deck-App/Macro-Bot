using MacroBot.Models.Webhook;

namespace MacroBot.ServiceInterfaces;

public interface IDiscordService
{
    public Task BroadcastWebhookAsync(WebhookItem webhook, WebhookRequest webhookRequest);
}