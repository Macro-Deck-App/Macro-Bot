using System.Text.Json.Serialization;

namespace MacroBot.Core.Models.BuyMeACoffee;

public class BuyMeACoffeeDonationCreatedData
{
    [JsonPropertyName("amount")]
    public float? Amount { get; set; }
    
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
    
    [JsonPropertyName("note_hidden")]
    public string? NoteHidden {get; set; }
    
    [JsonPropertyName("support_note")]
    public string? SupportNote { get; set; }
    
    [JsonPropertyName("supporter_name")]
    public string? SupporterName { get; set; }
}