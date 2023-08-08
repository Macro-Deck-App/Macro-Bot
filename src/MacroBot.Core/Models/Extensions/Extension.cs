using System.Text.Json.Serialization;

namespace MacroBot.Core.Models.Extensions;

public class Extension
{
    [JsonPropertyName("packageId")]
    public string PackageId { get; set; }
    [JsonPropertyName("extensionType")]
    public string ExtensionType { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("author")]
    public string Author { get; set; }
    [JsonPropertyName("gitHubRepository")]
    public string GithubRepository { get; set; }
    [JsonPropertyName("dSupportUserId")]
    public string? DSupportUserId { get; set; }
}