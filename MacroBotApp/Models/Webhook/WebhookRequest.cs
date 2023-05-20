namespace MacroBot.Models.Webhook;

public class WebhookRequest
{
	public string? Title { get; set; }
	public string? Text { get; set; }
	public bool? ToEveryone { get; set; }
	public WebhookRequestEmbed? Embed { get; set; }
}