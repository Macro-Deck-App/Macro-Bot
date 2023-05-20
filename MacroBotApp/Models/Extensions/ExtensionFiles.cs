using System.Text.Json.Serialization;

namespace MacroBot.Models.Extensions;

public class ExtensionFiles
{
	[JsonPropertyName("version")]
	public string Version { get; set; }

	[JsonPropertyName("minApiVersion")]
	public string MinimumApiVersion { get; set; }

	[JsonPropertyName("uploadDateTime")]
	private string _uploadDateTime { get; }

	[JsonIgnore] 
	public long UploadDateTime => DateTimeOffset.Parse(_uploadDateTime).ToUnixTimeMilliseconds();
}