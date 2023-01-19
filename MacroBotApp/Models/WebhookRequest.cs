namespace MacroBot.Models;

public class WebhookRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string ThumbnailUrl { get; set; }
    public string Content { get; set; }
    public string ImageUrl { get; set; }
    public bool ToEveryone { get; set; }
}