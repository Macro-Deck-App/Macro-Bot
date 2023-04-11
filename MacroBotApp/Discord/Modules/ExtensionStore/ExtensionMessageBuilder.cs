using System.Web;
using Discord;
using MacroBot.Extensions;
using MacroBot.Models.Extensions;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroBot.Discord.Modules.ExtensionStore;

public class ExtensionMessageBuilder
{
    private static readonly ILogger _logger = Log.ForContext<ExtensionMessageBuilder>();

    public static async Task<Embed> BuildProblemExtensionAsync(List<AllExtensions> extensionsList)
    {
        var embed = new EmbedBuilder {
            Title = $"Do you have problems with {((extensionsList.Count <= 1)? "this plugin or icon pack" : "these plugins or icon packs")}?",
            Description = "Macro Bot detected on your thread name or thread first post a name of a plugin or icon pack."
        };
        
        foreach (var ext in extensionsList)
        {
            try
            {
                var a = (ext.DSupportUserId is null) ? ext.Author : String.Format("<@{UserId}>", ext.DSupportUserId);
                embed.AddField(ext.Name, $"by {a}", true);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Can't add embed field");
            }
        }

        return embed.Build();
    }

    public static async Task<MessageComponent> BuildProblemExtensionInteractionAsync(List<AllExtensions> extensionsList)
    {
        var component = new ComponentBuilder();
        var selectMenu = new SelectMenuBuilder()
        {
            CustomId = "ProblemExtensionInteraction",
            MaxValues = extensionsList.Count,
            Placeholder = "Select one or more...",
            MinValues = 1
        };

        foreach (var ext in extensionsList)
        {
            try
            {
                var a = (ext.DSupportUserId is null) ? ext.Author : String.Format("<@{UserId}>", ext.DSupportUserId);
                selectMenu.AddOption(ext.Name, ext.PackageId, $"by {a}");
            }
            catch (Exception e)
            {
                _logger.Error(e, "Can't add component");
            }
        }

        component.WithSelectMenu(selectMenu);
        component.WithButton("No, thanks.", "ProblemExtensionButtonNo", ButtonStyle.Secondary);
        return component.Build();
    }
    
    public static async Task<Embed> BuildSearchExtensionAsync(IHttpClientFactory httpClientFactory, string query)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var extensions =
            await httpClient.GetFromJsonAsync<ExtensionResponse>(string.Format("https://test.extensionstore.api.macro-deck.app/rest/v2/extensions/search/{0}", HttpUtility.UrlEncode(query)));
        
        var embed = new EmbedBuilder {
            Title = String.Format((extensions.TotalItemsCount > 0) ? "Extension Results of '{0}'" : "Could not find extension '{0}'", query)
        };

        if (extensions.TotalItemsCount > 0)
        {
            foreach (var ext in extensions?.Data)
            {
                EmbedFieldBuilder field = new();
                field.WithName(String.Format("[{0}] {1}", ext.ExtensionType, ext.PackageId));
                field.WithValue(String.Format(
                    "{0}\r\n" +
                    "by {1}",
                    (ext.GithubRepository is not null)? string.Format("[{0}]({1})", ext.Name, ext.GithubRepository) : ext.Name,
                    (ext.DSupportUserId is not null)? string.Format("<@{0}>", ext.DSupportUserId) : ext.Author
                ));

                embed.AddField(field);
            }
        } else {
            embed.WithDescription(
                "We could not find it! You can try these:\r\n"+
                " - Check the name! Maybe there is a typo?\r\n"+
                " - Tell one of the Macro Deck 2 dev team member."
            );
        }

        return embed.Build();
    }
}
