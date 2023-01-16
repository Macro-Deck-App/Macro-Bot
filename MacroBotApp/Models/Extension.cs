using System.Text.Json.Serialization;

namespace MacroBot.Models;

public class Extension {
    [JsonPropertyName("packageId")]
    public string? PackageId;
    [JsonPropertyName("extensionType")]
    public string? ExtensionType;
    [JsonPropertyName("name")]
    public string? Name;
    [JsonPropertyName("author")]
    public string? Author;
    [JsonPropertyName("gitHubRepository")]
    public string? GithubRepository;
    [JsonPropertyName("dSupportUserId")]
    public ulong? DSupportUserId;
}