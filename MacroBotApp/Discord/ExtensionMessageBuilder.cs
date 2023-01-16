using System.Net.Http.Json;
using Discord;
using MacroBot.Models;

namespace MacroBot.Discord;

public static class ExtensionMessageBuilder
{

    public static async Task<Embed> BuildAllExtensionsAsync(IHttpClientFactory httpClientFactory,
        List<List<EmbedFieldBuilder>> fields)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var extensions = await httpClient.GetFromJsonAsync<List<Extension>>("https://extensionstore.api.macro-deck.app/Extensions");
        var embed = new EmbedBuilder()
            .WithTitle("Macro Deck Extensions")
            .WithDescription("This is the list of Macro Deck Extensions.");
        var f = 0;
        List<EmbedFieldBuilder> flds = new();
        foreach (var ext in extensions)
        {
            var extensionFile =
                await httpClient.GetFromJsonAsync<ExtensionFile>(
                    $"https://extensionstore.api.macro-deck.app/ExtensionsFiles/{ext.PackageId}@latest");
            
            if (f == 15) { fields.Add(flds); flds = new(); f = 0; }
            EmbedFieldBuilder field = new();
            field.WithName($"[{ext.ExtensionType}] {ext.PackageId}");
            var extensionVersionInfo = extensionFile != null
                ? $"Latest Version: {extensionFile.Version}\r\nMin API Version: {extensionFile.MinAPIVersion}"
                : string.Empty;
            field.WithValue(
                $"{(ext.GithubRepository is not null 
                    ? $"[{ext.Name}]({ext.GithubRepository})" 
                    : ext.Name)} by {(ext.DSupportUserId is not null 
                    ? $"<@{ext.DSupportUserId}>" 
                    : ext.Author)}\r\n" +
                (extensionFile is not null 
                    ? $"Latest Version: {extensionFile.Version}\r\n" +
                      $"Min API Version: {extensionFile.MinAPIVersion}"
                    : string.Empty));
            field.WithIsInline(true);
            flds.Add(field);
            f++;
        }
        if (!(f >= 15)) {
            fields.Add(flds);
        }
        embed.WithFields(fields[0]);
        return embed.Build();
    }

    public static async Task<Embed> BuildSearchExtensionAsync(IHttpClientFactory httpClientFactory, string query)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var extension = await httpClient.GetFromJsonAsync<Extension>(
            $"https://extensionstore.api.macro-deck.app/Extensions/{query}");

        if (extension is null)
        {
            var extensions =
                await httpClient.GetFromJsonAsync<List<Extension>>("https://extensionstore.api.macro-deck.app/Extensions");

            if (extensions != null)
            {
                extension = await extensions.ToAsyncEnumerable().FirstOrDefaultAsync(e =>
                    e.Name?.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0 &&
                    e.PackageId?.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0);
            }
        }
        
        var embed = new EmbedBuilder {
            Title = extension?.Name ?? "Not found"
        };

        if (extension is not null && !string.IsNullOrWhiteSpace(extension.PackageId))
        {
            embed.WithThumbnailUrl($"https://extensionstore.api.macro-deck.app/Extensions/Icon/{extension.PackageId}");
            embed.WithUrl(extension.GithubRepository);
            
            embed.AddField("Package ID", extension.PackageId);
            embed.AddField("Author", extension.DSupportUserId is not null
                ? $"<@{extension.DSupportUserId}> ({extension.Author})"
                : extension.Author, true);
            
            var extensionFile = await httpClient.GetFromJsonAsync<ExtensionFile>(
                $"https://extensionstore.api.macro-deck.app/ExtensionsFiles/{extension.PackageId}@latest");

            if (extensionFile is not null)
            {
                embed.AddField("Latest version", extensionFile.Version, true);
                embed.AddField("Min API version", extensionFile.MinAPIVersion, true);
            }
        }

        return embed.Build();
    }
    
}