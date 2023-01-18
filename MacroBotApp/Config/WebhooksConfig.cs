using System.Text.Json.Serialization;

namespace MacroBot.Config;

public class WebhooksConfig : LoadableConfig<WebhooksConfig>
{
    public List<WebhookItem> Items { get; set; } = new()
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
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public List<Field> Fields { get; set; } = new()
        {
            new Field
            {
                Name = "Test",
                Value = "Test value",
                Inline = false
            }
        };
        public string ImageUrl { get; set; }
    }

    public class Field
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Inline { get; set; }
    }
}