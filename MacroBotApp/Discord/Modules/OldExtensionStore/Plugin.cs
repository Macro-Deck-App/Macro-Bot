using System.Text.Json.Serialization;

namespace MacroBot.Discord.Modules.OldExtensionStore {
    public class Plugin
    {
        [JsonPropertyName("package-id")]
        public string PackageId { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonPropertyName("author-discord")]
        public string ADiscordID { get; set; }
        [JsonPropertyName("repository")]
        public string Repository { get; set; }
        [JsonPropertyName("target-api")]
        public string TargetAPI { get; set; }
    }
}