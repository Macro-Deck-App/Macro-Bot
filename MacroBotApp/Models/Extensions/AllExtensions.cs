using System.Text.Json.Serialization;

namespace MacroBot.Models.Extensions;

public class AllExtensions
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
    [JsonPropertyName("totalDownloads")]
    public int Downloads { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
}