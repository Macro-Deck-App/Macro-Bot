using System.Data.Common;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;

namespace MacroBot.Core.Discord.Modules.ExtensionStore;

// Temporarely disabled

[Group("extensions", "Extension Store Commands")]
[UsedImplicitly]
public class ExtensionCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    
    private static List<Embed> _allPluginEmbeds { get; set; }

    public ExtensionCommands(IHttpClientFactory httpClientFactory, DiscordSocketClient client)
    {
        _httpClientFactory = httpClientFactory;
        client.ButtonExecuted += ClientOnButtonExecuted;
    }

    [SlashCommand("get", "Get a plugin")]
    public async Task GetPlugin([Summary(description: "Extension Name or Package ID")] string search) {
        await DeferAsync(ephemeral: true);
        var embed = await ExtensionMessageBuilder.BuildSearchExtensionAsync(_httpClientFactory, search);
        await FollowupAsync(embed: embed, ephemeral: true);
    }
    
    [SlashCommand("browse", "Get all plugins")]
    public async Task GetPlugins([Summary(description: "Include plugins?")] bool includePlugins = true, [Summary(description: "Include icon packs?")] bool includeIconPacks = true) {
        await DeferAsync(ephemeral: true);
        var embeds = await ExtensionMessageBuilder.BuildAllExtensionsAsync(_httpClientFactory, includePlugins, includeIconPacks);
        _allPluginEmbeds = embeds;
        var component = new ComponentBuilder()
            .WithButton("<", "exst-left", ButtonStyle.Success, disabled: true)
            .WithButton($"1 / {embeds.Count}", "exst-plc", ButtonStyle.Secondary, disabled: true)
            .WithButton(">", "exst-right", ButtonStyle.Success);
        await FollowupAsync(embed: embeds[0], components: component.Build(), ephemeral: true);
    }

    private async Task ClientOnButtonExecuted(SocketMessageComponent msg)
    {
        try {
            await PlBtnExecuted(msg, msg.User);
        } catch {}
    }
    
    private async Task PlBtnExecuted(SocketMessageComponent smc, IPresence user) {
        var id = smc.Data.CustomId;

        if (smc.User != user)
        {
            return;
        }

        try {
            var content = smc.Message.Embeds.ToArray()[0].Footer!.Value.Text.Replace("Page ", "");
            var i = 0;

            switch (id)
            {
                case "exst-left":
                    i = Convert.ToInt32(content) - 1;
                    break;
                case "exst-right":
                    i = Convert.ToInt32(content) + 1;
                    break;
                default:
                    return;
            }

            await smc.DeferAsync(ephemeral: true);

            var embed = _allPluginEmbeds[i - 1].ToEmbedBuilder();

            var builder = new ComponentBuilder()
                .WithButton("<", "exst-left", ButtonStyle.Success, disabled: (i is 1))
                .WithButton($"{i} / {_allPluginEmbeds.Count}", "exst-plc", ButtonStyle.Secondary, disabled: true)
                .WithButton(">", "exst-right", ButtonStyle.Success, disabled: (i == _allPluginEmbeds.Count));

            await smc.ModifyOriginalResponseAsync(msg => {
                msg.Embed = embed.Build();
                msg.Components = builder.Build();
            });
        } catch {}
    }
}



