namespace MacroBot.Models.Webhook;

public class WebhookRequestEmbedField
{
	public string Name { get; set; }
	public string Value { get; set; }
	public bool? Inline { get; set; } = false;
}