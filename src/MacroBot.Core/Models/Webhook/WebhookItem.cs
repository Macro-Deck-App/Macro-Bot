namespace MacroBot.Core.Models.Webhook;

public class WebhookItem
{
    public string Id { get; set; } = string.Empty;
    public string BearerAuthKey { get; set; } = string.Empty;
    public ulong ChannelId { get; set; }
}