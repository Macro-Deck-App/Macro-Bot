using MacroBot.Core.Models.Webhook;

namespace MacroBot.Core.Config;

public class WebhooksConfig : LoadableConfig<WebhooksConfig>
{
    public List<WebhookItem> Webhooks { get; set; } = new()
    {
        new WebhookItem
        {
            Id = "test",
            BearerAuthKey = "",
        }
    };
    
    
}