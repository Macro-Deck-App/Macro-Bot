using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Discord;
using MacroBot.Core.Extensions;
using MacroBot.Core.Models.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroBot.Core.Discord.Modules.ExtensionStore;

public class ExtensionMessageBuilder
{
    private static readonly ILogger _logger = Log.ForContext<ExtensionMessageBuilder>();

    public static async Task<List<Embed>> BuildAllExtensionsAsync(IHttpClientFactory httpClientFactory, bool includePlugins = true, bool includeIconPacks = true)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var extensions =
            await HttpClientJsonExtensions.GetFromJsonAsync<ExtensionsResponse>(httpClient, String.Format("https://extensionstore.api.macro-deck.app/rest/v2/extensions?pageSize=100&ShowPlugins={0}&ShowIconPacks={1}", includePlugins, includeIconPacks));
        
        var embed = new EmbedBuilder()
            .WithTitle("Macro Deck Extensions")
            .WithDescription("This is the list of Macro Deck Extensions.")
            .WithFooter($"Page 1");
        var fields = new List<EmbedFieldBuilder>();
        if (extensions != null) {
            _logger.Information("Loaded {NoExtensions} extensions", extensions.Items.Count);

            foreach (var ext in extensions.Items)
            {
                EmbedFieldBuilder field = new();
                field.WithName($"{(includePlugins && includeIconPacks ? $"[{(ext.ExtensionType is 0? "Plugin" : "Icon Pack")}] " : "")}{ext.PackageId}");
                field.WithValue(
                    $"{(ext.GitHubRepository is not null
                        ? $"[{ext.Name}]({ext.GitHubRepository})"
                        : ext.Name)} by {(!string.IsNullOrWhiteSpace(ext.DSupportUserId)
                        ? $"<@{ext.DSupportUserId}>"
                        : ext.Author)}\r\n");
                field.WithIsInline(true);
                fields.Add(field);
            }
        }

        List<Embed> embedList = new List<Embed>();

        foreach (EmbedFieldBuilder[] flds in fields.Chunk(24)) {
            embed.WithFields(flds);
            embedList.Add(embed.Build());
            embed = new EmbedBuilder()
                .WithTitle("Macro Deck Extensions")
                .WithDescription("This is the list of Macro Deck Extensions.")
                .WithFooter($"Page {embedList.Count + 1}");
        }

        return embedList;
    }

    public static async Task<Embed> BuildSearchExtensionAsync(IHttpClientFactory httpClientFactory, string query)
    {
        using var httpClient = httpClientFactory.CreateClient();
        Extension? extension = new();

        var extensions =
            await HttpClientJsonExtensions.GetFromJsonAsync<ExtensionsResponse>(httpClient, "https://extensionstore.api.macro-deck.app/rest/v2/extensions?pageSize=100");

        if (extensions != null)
        {
            extension = await extensions.Items.ToAsyncEnumerable().FirstOrDefaultAsync(e =>
                e.Name?.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > 0 ||
                e.PackageId?.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) > 0);
        }
        
        var embed = new EmbedBuilder
        {
            Title = extension?.Name ?? $"Cannot find '{query}'",
            Description = extension?.Description ?? "Please double-check your query."
        };

        if (extension is not null && !string.IsNullOrWhiteSpace(extension.PackageId))
        {
            embed.WithThumbnailUrl($"https://extensionstore.api.macro-deck.app/rest/v2/extensions/icon/{extension.PackageId}");
            embed.WithUrl(extension.GitHubRepository);
            
            embed.AddField("Package ID", extension.PackageId, true);
            embed.AddField("Author", !string.IsNullOrWhiteSpace(extension.DSupportUserId)
                ? $"<@{extension.DSupportUserId}> ({extension.Author})"
                : extension.Author, true);
            
            var extensionFile = await httpClient.GetExtensionAsync(extension.PackageId);

            if (extensionFile is not null)
            {
                var efiles = extensionFile.ExtensionFiles.OrderByDescending(x => x.UploadDateTime).ToArray();
                var offset = new DateTimeOffset(efiles[0].UploadDateTime);

                if (efiles[0].LicenseName is not "Not found")
                    embed.AddField("License", $"[{efiles[0].LicenseName}]({efiles[0].LicenseUrl})", true);

                embed.AddField("Release Date", $"<t:{offset.ToUnixTimeSeconds()}:F>");
                embed.AddField("Extension Type", extension.ExtensionType is 0? "Plugin" : "Icon Pack", true);
                embed.AddField("Latest version", efiles[0].Version, true);
                embed.AddField("Min API version", efiles[0].MinApiVersion, true);
            }
        }

        return embed.Build();
    }
    
    public static async Task<ExtensionDetection> BuildExtensionDetectionAsync(IHttpClientFactory httpClientFactory, string title, string content)
    {
        ExtensionDetection extension = new();

        using var httpClient = httpClientFactory.CreateClient();
        List<Extension> exts = new();

        try {
            var extensions =
                await HttpClientJsonExtensions.GetFromJsonAsync<ExtensionsResponse>(httpClient, "https://extensionstore.api.macro-deck.app/rest/v2/extensions?pageSize=100");

            if (extensions != null)
            {
                exts = extensions.Items
                    .Where(e => IsMatch(title, e.Name) || IsMatch(title, e.PackageId) || IsMatch(content, e.Name) || IsMatch(content, e.PackageId))
                    .ToList();
            }

            if (exts is not null && exts.Count is not 0)
            {
                var embed = new EmbedBuilder
                {
                    Title = $"Do you have problems on {(exts.Count == 1? $"this {(exts[0].ExtensionType == 0? "plugin" : "icon pack")}?" : "these plugins or icon packs?")}"
                };

                if (exts.Count == 1) {
                    embed.WithDescription($"# {exts[0].Name}");
                    embed.AddField("Author", exts[0].Author);
                    extension.Embeds = embed.Build();
                    extension.Component = new ComponentBuilder()
                        .WithButton("Yes", "extd-" + exts[0].PackageId, ButtonStyle.Success)
                        .WithButton("No", "extd-no", ButtonStyle.Danger)
                        .Build();
                } else {
                    List<EmbedFieldBuilder> fields = new();
                    List<SelectMenuOptionBuilder> options = new();

                    foreach (var ext in exts) {
                        fields.Add(new EmbedFieldBuilder() {
                            Name = ext.Name,
                            Value = $"by {ext.Author}",
                            IsInline = true
                        });
                        options.Add(new SelectMenuOptionBuilder()
                            .WithLabel(ext.Name)
                            .WithDescription($"by {ext.Author}")
                            .WithValue("extd-" + ext.PackageId));
                    }

                    if (fields.Count > 25) {
                        List<Embed> embeds = new();
                        embed.WithFields(fields.Chunk(25).ToArray()[0]);
                        embed.WithFooter($"25 of {fields.Count}");
                        extension.Embeds = embed.Build();
                    } else {
                        embed.WithFields(fields);
                        extension.Embeds = embed.Build();
                    }

                    extension.Component = new ComponentBuilder()
                        .WithSelectMenu(new SelectMenuBuilder()
                            .WithOptions(options.Chunk(25).ToList()[0].ToList())
                            .WithCustomId("extd-selmenu")
                            .WithMinValues(1)
                            .WithMaxValues(options.Chunk(25).ToList()[0].Count())
                            .WithPlaceholder("Select a plugin"))
                        .WithButton("It isn't any of those.", "extd-no", ButtonStyle.Danger)
                        .Build();
                }   
            }

            return extension;
        } catch (Exception e) {
            Log.Error(e, "Cannot create extension detection message");
            return null;
        }
    }

    static bool IsMatch(string text, string keyword)
    {
        bool b = false;
        foreach (var str in keyword.Replace(" Plugin", "").Split(" ")) {
            b = text.ToLower().Contains(str.ToLower());
        }
        return b;
    }

    public class ExtensionDetection {
        public Embed Embeds { get; set; }
        public MessageComponent Component { get; set; }
    }
}