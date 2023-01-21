using Newtonsoft.Json;

namespace MacroBot.Discord.Modules.OldExtensionStore {
    public class Plugin
    {
        [JsonProperty(PropertyName = "package-id")]
        public string package_id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public string author { get; set; }
        [JsonProperty(PropertyName = "author-discord")]
        public string author_discord { get; set; }
        public string repository { get; set; }
        [JsonProperty(PropertyName = "target-api")]
        public string target_api { get; set; }
    }
}