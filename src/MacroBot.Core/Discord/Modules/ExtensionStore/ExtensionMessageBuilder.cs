using System.Net.Http.Json;
using Discord;
using MacroBot.Core.Models.Extensions;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroBot.Core.Discord.Modules.ExtensionStore;

public class ExtensionMessageBuilder
{
    private static readonly ILogger _logger = Log.ForContext<ExtensionMessageBuilder>();

    public static async Task<Embed> BuildAllExtensionsAsync(IHttpClientFactory httpClientFactory)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var extensions =
            await HttpClientJsonExtensions.GetFromJsonAsync<List<Extension>>(httpClient, "https://extensionstore.api.macro-deck.app/Extensions");

        _logger.Information("Loaded {NoExtensions} extensions", extensions?.Count);
        
        var embed = new EmbedBuilder()
            .WithTitle("Macro Deck Extensions")
            .WithDescription("This is the list of Macro Deck Extensions.");
        var fields = new List<EmbedFieldBuilder>();
        if (extensions != null)
            foreach (var ext in extensions)
            {
                EmbedFieldBuilder field = new();
                field.WithName($"[{ext.ExtensionType}] {ext.PackageId}");
                field.WithValue(
                    $"{(ext.GithubRepository is not null
                        ? $"[{ext.Name}]({ext.GithubRepository})"
                        : ext.Name)} by {(!string.IsNullOrWhiteSpace(ext.DSupportUserId)
                        ? $"<@{ext.DSupportUserId}>"
                        : ext.Author)}\r\n");
                field.WithIsInline(true);
                fields.Add(field);
            }

        embed.WithFields(fields[0]);
        return embed.Build();
    }

    public static async Task<Embed> BuildSearchExtensionAsync(IHttpClientFactory httpClientFactory, string query)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var extension = await HttpClientJsonExtensions.GetFromJsonAsync<Extension>(httpClient, $"https://extensionstore.api.macro-deck.app/Extensions/{query}");

        if (extension is null)
        {
            var extensions =
                await HttpClientJsonExtensions.GetFromJsonAsync<List<Extension>>(httpClient, "https://extensionstore.api.macro-deck.app/Extensions");

            if (extensions != null)
            {
                extension = await extensions.ToAsyncEnumerable().FirstOrDefaultAsync(e =>
                    e.Name?.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0 &&
                    e.PackageId?.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0);
            }
        }
        
        var embed = new EmbedBuilder
        {
            Title = extension?.Name ?? "Not found"
        };

        if (extension is not null && !string.IsNullOrWhiteSpace(extension.PackageId))
        {
            embed.WithThumbnailUrl($"https://extensionstore.api.macro-deck.app/Extensions/Icon/{extension.PackageId}");
            embed.WithUrl(extension.GithubRepository);
            
            embed.AddField("Package ID", extension.PackageId);
            embed.AddField("Author", !string.IsNullOrWhiteSpace(extension.DSupportUserId)
                ? $"<@{extension.DSupportUserId}> ({extension.Author})"
                : extension.Author, true);
            
            var extensionFile = await HttpClientJsonExtensions.GetFromJsonAsync<ExtensionFile>(httpClient, $"https://extensionstore.api.macro-deck.app/ExtensionsFiles/{extension.PackageId}@latest");

            if (extensionFile is not null)
            {
                embed.AddField("Latest version", extensionFile.Version, true);
                embed.AddField("Min API version", extensionFile.MinApiVersion, true);
            }
        }

        return embed.Build();
    }
    
}