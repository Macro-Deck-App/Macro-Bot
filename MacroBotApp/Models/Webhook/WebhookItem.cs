namespace MacroBot.Models.Webhook;

public class WebhookItem
{
	public string Id { get; set; }
	public string BearerAuthKey { get; set; }
	public ulong ChannelId { get; set; }
}