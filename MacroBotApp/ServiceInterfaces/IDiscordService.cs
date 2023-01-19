using MacroBot.Models;

namespace MacroBot.ServiceInterfaces;

public interface IDiscordService
{
    public Task BroadcastWebhookAsync(ulong channelId, WebhookRequest webhookRequest);
}