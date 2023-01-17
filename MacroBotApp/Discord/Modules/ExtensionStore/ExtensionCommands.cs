using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MacroBot.Config;
// ReSharper disable UnusedMember.Global

// ReSharper disable UnusedType.Global

namespace MacroBot.Discord.Modules.ExtensionStore;

[Group("extension", "Extension Store Commands")]
public class ExtensionCommands : InteractionModuleBase<SocketInteractionContext> {
    
    private readonly BotConfig _botConfig;
    private readonly CommandsConfig _commandsConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    
    
    private List<List<EmbedFieldBuilder>> _allPluginFields = new();

    public ExtensionCommands(BotConfig botConfig, 
        CommandsConfig commandsConfig, 
        IHttpClientFactory httpClientFactory)
    {
        _botConfig = botConfig;
        _commandsConfig = commandsConfig;
        _httpClientFactory = httpClientFactory;
    }

    [SlashCommand("get", "Get a plugin")]
    public async Task GetPlugin([Summary(description: "Extension Name or Package ID")] string query) {
        await DeferAsync();
        var embed = await ExtensionMessageBuilder.BuildSearchExtensionAsync(_httpClientFactory, query);
        await FollowupAsync(embed: embed);
    }

    [SlashCommand("getall", "Get all plugins")]
    public async Task GetPlugins() {
        _allPluginFields = new List<List<EmbedFieldBuilder>>();
        await DeferAsync();
        var embed = await ExtensionMessageBuilder.BuildAllExtensionsAsync(_httpClientFactory, _allPluginFields);
        Context.Client.ButtonExecuted -= ClientOnButtonExecuted;
        Context.Client.ButtonExecuted += ClientOnButtonExecuted;
        var builder = new ComponentBuilder()
            .WithButton("<", "plugin-list-page-back", ButtonStyle.Success, disabled: true)
            .WithButton($"1 / {_allPluginFields.Count}", "page", ButtonStyle.Secondary, disabled: true)
            .WithButton(">", "plugin-list-page-forward", ButtonStyle.Success, disabled: (_allPluginFields.Count is 1));

        await FollowupAsync("1", embed: embed, components: builder.Build());
    }

    private async Task ClientOnButtonExecuted(SocketMessageComponent msg)
    {
        await PlBtnExecuted(msg, Context.User);
    }
    
    private async Task PlBtnExecuted(SocketMessageComponent smc, IPresence user) {
        if (smc.User != user) { return; }
        await smc.DeferAsync();

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

        var embed = new EmbedBuilder()
            .WithTitle("Macro Deck Extensions")
            .WithDescription("This is the list of Macro Deck Extensions.")
            .WithFields(_allPluginFields[i - 1]);

        var builder = new ComponentBuilder()
            .WithButton("<", "plugin-list-page-back", ButtonStyle.Success, disabled: (i is 1))
            .WithButton($"{i} / {_allPluginFields.Count}", "page", ButtonStyle.Secondary, disabled: true)
            .WithButton(">", "plugin-list-page-forward", ButtonStyle.Success, disabled: (i == _allPluginFields.Count));

        await smc.Message.ModifyAsync(msg => {
            msg.Content = $"{i}";
            msg.Embed = embed.Build();
            msg.Components = builder.Build();
        });
    }
}



