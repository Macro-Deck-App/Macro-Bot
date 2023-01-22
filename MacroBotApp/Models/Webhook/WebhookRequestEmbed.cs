namespace MacroBot.Models.Webhook;

public class WebhookRequestEmbed
{
    public WebhookRequestEmbedColor? Color { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<WebhookRequestEmbedField>? Fields { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? ImageUrl { get; set; }
}