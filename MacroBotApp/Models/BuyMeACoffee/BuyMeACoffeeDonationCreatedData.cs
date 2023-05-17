using System.Text.Json.Serialization;

namespace MacroBot.Models.BuyMeACoffee;

public class BuyMeACoffeeDonationCreatedData
{
<<<<<<< HEAD
    [JsonPropertyName("amount")] public float? Amount { get; set; }

    [JsonPropertyName("status")] public string? Status { get; set; }

    [JsonPropertyName("currency")] public string? Currency { get; set; }

    [JsonPropertyName("note_hidden")] public bool NoteHidden { get; set; }

    [JsonPropertyName("support_note")] public string? SupportNote { get; set; }

    [JsonPropertyName("supporter_name")] public string? SupporterName { get; set; }

    public bool Succeeded => Status?.Equals("succeeded", StringComparison.InvariantCultureIgnoreCase) ?? false;
=======
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
>>>>>>> 9eb4fad4dcae341cb92e06706d6e23ec748ddf0b
}