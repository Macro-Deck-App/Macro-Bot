using System.Text.Json.Serialization;

namespace MacroBot.Models;

public class ExtensionFile {
    [JsonPropertyName("version")]
    public string Version { get; set; }
    [JsonPropertyName("minApiVersion")]
    public int MinApiVersion { get; set; }
}