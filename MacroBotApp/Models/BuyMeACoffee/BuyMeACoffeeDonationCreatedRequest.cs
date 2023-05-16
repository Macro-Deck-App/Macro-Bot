using System.Text.Json.Serialization;

namespace MacroBot.Models.BuyMeACoffee;

public class BuyMeACoffeeDonationCreatedRequest
{
    [JsonPropertyName("type")] public string? Type { get; set; }

    [JsonPropertyName("data")] public BuyMeACoffeeDonationCreatedData? Data { get; set; }
}