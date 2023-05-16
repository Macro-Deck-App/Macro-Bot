using System.Web;
using Discord;
using MacroBot.Config;
using MacroBot.Extensions;
using MacroBot.Models.Extensions;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroBot.Discord.Modules.ExtensionStore;

public class ExtensionMessageBuilder
{
    private static readonly ILogger Logger = Log.ForContext<ExtensionMessageBuilder>();

    public static Embed BuildProblemExtensionAsync(List<AllExtensions> extensionsList)
    {
        var embed = new EmbedBuilder
        {
            Title =
                $"Do you have problems with {(extensionsList.Count <= 1 ? "this plugin or icon pack" : "these plugins or icon packs")}?",
            Description = "Macro Bot detected on your thread name or thread first post a name of a plugin or icon pack."
        };

        foreach (var ext in extensionsList)
            try
            {
                var a = ext.DSupportUserId is null ? ext.Author : string.Format("<@{UserId}>", ext.DSupportUserId);
                embed.AddField(ext.Name, $"by {a}", true);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Can't add embed field");
            }

        return embed.Build();
    }

    public static MessageComponent BuildProblemExtensionInteractionAsync(List<AllExtensions> extensionsList)
    {
        var component = new ComponentBuilder();
        var selectMenu = new SelectMenuBuilder
        {
            CustomId = "ProblemExtensionInteraction",
            MaxValues = extensionsList.Count,
            Placeholder = "Select one or more...",
            MinValues = 1
        };

        foreach (var ext in extensionsList)
            try
            {
                var a = ext.DSupportUserId is null ? ext.Author : string.Format("<@{UserId}>", ext.DSupportUserId);
                selectMenu.AddOption(ext.Name, ext.PackageId, $"by {a}");
            }
            catch (Exception e)
            {
                Logger.Error(e, "Can't add component");
            }

        component.WithSelectMenu(selectMenu);
        component.WithButton("No, thanks.", "ProblemExtensionButtonNo", ButtonStyle.Secondary);
        return component.Build();
    }

    public static async Task<Embed> BuildSearchExtensionAsync(IHttpClientFactory httpClientFactory,
        ExtensionDetectionConfig extDetectionConfig,
        string query)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var extensions =
            await httpClient.GetFromJsonAsync<ExtensionResponse>(
                $"{extDetectionConfig.SearchExtensionsUrl}/{HttpUtility.UrlEncode(query)}");

        var embed = new EmbedBuilder
        {
            Title = string.Format(
                extensions != null && extensions.TotalItemsCount > 0
                    ? "Extension Results of '{0}'"
                    : "Could not find extension '{0}'", query)
        };

        if (extensions != null && extensions.TotalItemsCount > 0)
        {
            if (extensions.Data == null) return embed.Build();
            foreach (var ext in extensions.Data)
            {
                EmbedFieldBuilder field = new();
                field.WithName($"[{ext.ExtensionType}] {ext.PackageId}");
                field.WithValue(string.Format(
                    "{0}\r\n" +
                    "by {1}",
                    ext.GithubRepository.IsNullOrWhiteSpace()
                        ? $"[{ext.Name}]({ext.GithubRepository})"
                        : ext.Name,
                    ext.DSupportUserId is not null ? $"<@{ext.DSupportUserId}>" : ext.Author
                ));

                embed.AddField(field);
            }
        }
        else
        {
            embed.WithDescription(
                "We could not find it! You can try these:\r\n" +
                " - Check the name! Maybe there is a typo?\r\n" +
                " - Tell one of the Macro Deck 2 dev team member."
            );
        }

        return embed.Build();
    }
}