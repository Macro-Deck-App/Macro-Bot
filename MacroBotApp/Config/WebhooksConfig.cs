using System.Text.Json.Serialization;

namespace MacroBot.Config;

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
    
    public class WebhookItem
    {
        public string Id { get; set; }
        public string BearerAuthKey { get; set; }
        public ulong ChannelId { get; set; }
    }
}