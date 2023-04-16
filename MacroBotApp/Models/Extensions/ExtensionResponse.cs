using System.Text.Json.Serialization;

namespace MacroBot.Models.Extensions;

public class ExtensionResponse {
    [JsonPropertyName("totalItemsCount")]
    public int TotalItemsCount { get; set; }
    [JsonPropertyName("data")]
    public List<AllExtensions> Data { get; set; }
    [JsonPropertyName("maxPages")]
    public int MaxPages { get; set; }
}