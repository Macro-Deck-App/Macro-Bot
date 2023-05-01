using System.Text.Json.Serialization;

namespace MacroBot.Models.Translate;

public class Translated
{
    [JsonPropertyName("detectedLanguage")] public Detection DetectedLanguage { get; set; }

    [JsonPropertyName("translatedText")] public string TranslatedText { get; set; }
}