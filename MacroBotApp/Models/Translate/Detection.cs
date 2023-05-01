using System.Text.Json.Serialization;

namespace MacroBot.Models.Translate;

public class Detection
{
    [JsonPropertyName("confidence")]
    public float Confidence { get; set; }
    [JsonPropertyName("language")]
    public string Language { get; set; }
}