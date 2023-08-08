using MacroBot.Core.Models.Webhook;

namespace MacroBot.Core.ServiceInterfaces;

public interface IDiscordService
{
    public Task BroadcastWebhookAsync(WebhookItem webhook, WebhookRequest webhookRequest);
    public bool DiscordReady { get; }
}