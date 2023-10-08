using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MacroBot.Core.Models.Extensions;

public class ExtensionFile
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    [JsonPropertyName("minApiVersion")]
    public int MinApiVersion { get; set; } = 0;

    [JsonPropertyName("packageFileName")]
    public string PackageFileName { get; set; } = "";

    [JsonPropertyName("iconFileName")]
    public string IconFileName { get; set; } = "";

    [JsonPropertyName("readme")]
    public string Readme { get; set; } = "";

    [JsonPropertyName("fileHash")]
    public string FileHash { get; set; } = "";

    [JsonPropertyName("licenseName")]
    public string LicenseName { get; set; } = "";

    [JsonPropertyName("licenseUrl")]
    public string LicenseUrl { get; set; } = "";

    [JsonPropertyName("uploadDateTime")]
    public DateTime UploadDateTime { get; set; } = DateTime.MinValue;
}

public class Extension
{
    [JsonPropertyName("packageId")]
    public string PackageId { get; set; } = "";

    [JsonPropertyName("extensionType")]
    public int ExtensionType { get; set; } = 0;

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("author")]
    public string Author { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("gitHubRepository")]
    public string GitHubRepository { get; set; } = "";

    [JsonPropertyName("dSupportUserId")]
    public string? DSupportUserId { get; set; }

    [JsonPropertyName("extensionFiles")]
    public List<ExtensionFile> ExtensionFiles { get; set; } = new List<ExtensionFile>();
}

public class ExtensionsResponse
{
    [JsonPropertyName("items")]
    public List<Extension> Items { get; set; } = new List<Extension>();

    [JsonPropertyName("page")]
    public int Page { get; set; } = 0;

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 0;

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; } = 0;
}

public class TopDownloadsResponse : List<Extension>
{
}

public class CategoriesResponse : List<string>
{
}
