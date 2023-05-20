using MacroBot.Models.Webhook;

namespace MacroBot.Config;

public class WebhooksConfig : LoadableConfig<WebhooksConfig>
{
	public List<WebhookItem> Webhooks { get; set; } = new()
	{
		new WebhookItem
		{
			Id = "test",
			BearerAuthKey = string.Empty
		}
	};
}