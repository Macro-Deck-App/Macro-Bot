using Discord.Interactions;
using JetBrains.Annotations;

namespace MacroBot.Discord.Modules.ExtensionStore;

// Temporarely disabled

[Group("extensions", "Extension Store Commands")]
[UsedImplicitly]
public class ExtensionCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly BotConfig _botConfig;
    private readonly CommandsConfig _commandsConfig;
    private readonly ExtensionDetectionConfig _extDetectionConfig;
    private readonly IHttpClientFactory _httpClientFactory;

    public ExtensionCommands(BotConfig botConfig, 
        CommandsConfig commandsConfig, 
        ExtensionDetectionConfig extDetectionConfig,
        IHttpClientFactory httpClientFactory,
        DiscordSocketClient client)
    {
        _botConfig = botConfig;
        _commandsConfig = commandsConfig;
        _extDetectionConfig = extDetectionConfig;
        _httpClientFactory = httpClientFactory;
    }

    [SlashCommand("search", "Search plugins")]
    public async Task SearchPlugin([Summary(description: "Extension Name or Package ID")] string search) {
        await DeferAsync(ephemeral: true);
        var embed = await ExtensionMessageBuilder.BuildSearchExtensionAsync(_httpClientFactory, _extDetectionConfig, search);
        await FollowupAsync(embed: embed, ephemeral: true);
    }
    
    // This is currently disabled due to Discord API limitations.
    // In the current Discord API version, you can't edit Ephemeral messages.
    /*
    private async Task ClientOnButtonExecuted(SocketMessageComponent msg)
    {
        await PlBtnExecuted(msg, Context.User);
    }
    
    private async Task PlBtnExecuted(SocketMessageComponent smc, IPresence user) {
        if (smc.User != user)
        {
            return;
        }

        var content = smc.Message.CleanContent;
        var id = smc.Data.CustomId;
        var i = 0;

        switch (id)
        {
            case "plugin-list-page-back":
                i = Convert.ToInt32(content) - 1;
                break;
            case "plugin-list-page-forward":
                i = Convert.ToInt32(content) + 1;
                break;
            default:
                return;
        }

        await smc.DeferAsync(ephemeral: true);
        var extEmbed = await ExtensionMessageBuilder.BuildAllExtensionsAsync(_httpClientFactory, i);
        var component = new ComponentBuilder()
            .WithButton(emote: new Emoji("◀️"), customId: "plugin-list-page-back", disabled: (i is 1))
            .WithButton(string.Format("{0} / {1}", i, extEmbed.MaxPages), customId: "placeholder", style: ButtonStyle.Secondary, disabled: true)
            .WithButton(emote: new Emoji("▶️"), customId: "plugin-l
        await smc.Message.ModifyAsync(msg => {ist-page-forward", disabled: (i >= extEmbed.MaxPages));

            msg.Content = $"{i}";
            msg.Embed = extEmbed.Page;
            msg.Components = component.Build();
        });
    }
    */
}



