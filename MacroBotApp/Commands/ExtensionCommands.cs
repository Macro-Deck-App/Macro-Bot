using Discord.Interactions;
using Octokit;
using Discord.WebSocket;
using Discord;
using MacroBot.Config;
using MacroBot.Utils;
using Newtonsoft.Json;

namespace MacroBot.Commands;

[Group("extension", "Extension Store Commands")]
public class ExtensionCommands : InteractionModuleBase<SocketInteractionContext> {
    private readonly BotConfig _botConfig;
    private readonly CommandsConfig _commandsConfig;

    public ExtensionCommands(BotConfig botConfig, CommandsConfig commandsConfig)
    {
        _botConfig = botConfig;
        _commandsConfig = commandsConfig;
    }
    
    [SlashCommand("get", "Get a plugin")]
    public async Task getPlugin([Summary(description: "Extension Name or Package ID")] string query) {
        await DeferAsync();
        Extension extension = new();
        try {
            extension = JsonConvert.DeserializeObject<Extension>(await HttpRequest.GetAsync($"https://extensionstore.api.macro-deck.app/Extensions/{query}"));
        } catch (System.Net.WebException) {
            var extensions = JsonConvert.DeserializeObject<List<Extension>>(await HttpRequest.GetAsync($"https://extensionstore.api.macro-deck.app/Extensions"));
            foreach (var ext in extensions) {
                if ((ext.name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) || (ext.packageId.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)) {
                    extension = ext;
                    break;
                }
            }
        }

        var embed = new EmbedBuilder() {
            Title = $"{extension.name}"
        }; 

        ExtensionFile extfile = new();
        try {
            extfile = JsonConvert.DeserializeObject<ExtensionFile>(await HttpRequest.GetAsync($"https://extensionstore.api.macro-deck.app/ExtensionsFiles/{extension.packageId}@latest"));
        } catch (Exception) {}

        if (!String.IsNullOrEmpty(extension.githubRepository)) {
            var client = new GitHubClient(new ProductHeaderValue("DiscordBot"));
            client.Credentials = new Credentials(_botConfig.GithubToken);
            var repo = extension.githubRepository.Replace("https://github.com", "").Split("/").ToList();
            var repository = await client.Repository.Get(repo[1], repo[2]);

            if (repository.Description is not null)
                embed.WithDescription(repository.Description);

            embed.WithThumbnailUrl($"{extension.githubRepository}/raw/{repository.DefaultBranch}/ExtensionIcon.png");
            embed.WithUrl(extension.githubRepository);
        }

        try {
            embed.AddField("Package ID", extension.packageId);
        } catch (Exception) {}
        try {
            embed.AddField("Author", (extension.dSupportUserId is not null)? $"<@{extension.dSupportUserId}> ({extension.author})" : extension.author, true);
        } catch (Exception) {}
        try {
            embed.AddField("Latest Version", extfile.version, true);
        } catch (Exception) {}
        try {
            embed.AddField("Min API Version", extfile.minAPIVersion, true);
        } catch (Exception) {}

        await FollowupAsync(embed: embed.Build());
    }

    public List<List<EmbedFieldBuilder>> fields = new();        

    [SlashCommand("getall", "Get all plugins")]
    public async Task getPlugins() {
        fields = new();
        await DeferAsync();
        var extension = JsonConvert.DeserializeObject<List<Extension>>(await HttpRequest.GetAsync($"https://extensionstore.api.macro-deck.app/Extensions"));
        var embed = new EmbedBuilder()
            .WithTitle("Macro Deck Extensions")
            .WithDescription("This is the list of Macro Deck Extensions.");
        var f = 0;
        List<EmbedFieldBuilder> flds = new();
        foreach (var ext in extension) {
            ExtensionFile extfile = new();
            try {
                extfile = JsonConvert.DeserializeObject<ExtensionFile>(await HttpRequest.GetAsync($"https://extensionstore.api.macro-deck.app/ExtensionsFiles/{ext.packageId}@latest"));
            } catch (Exception) {}
            if (f == 15) { fields.Add(flds); flds = new(); f = 0; }
            EmbedFieldBuilder field = new();
            field.WithName($"[{ext.extensionType}] {ext.packageId}");
            var extinfof = "";
            try {
                extinfof = $"Latest Version: {extfile.version}\r\nMin API Version: {extfile.minAPIVersion}";
            } catch (Exception) {}
            field.WithValue($"{((ext.githubRepository is not null)? $"[{ext.name}]({ext.githubRepository})" : ext.name)} by {((ext.dSupportUserId is not null)? $"<@{ext.dSupportUserId}>" : ext.author)}\r\n{extinfof}");
            field.WithIsInline(true);
            flds.Add(field);
            f++;
            Console.WriteLine(f);
        }
        if (!(f >= 15)) {
            fields.Add(flds);
        }
        embed.WithFields(fields[0]);

        Context.Client.ButtonExecuted -= async (msg) => await PLBtnExecuted(msg, Context.User);
        Context.Client.ButtonExecuted += async (msg) => await PLBtnExecuted(msg, Context.User);

        var builder = new ComponentBuilder()
            .WithButton("<", "plugin-list-page-back", ButtonStyle.Success, disabled: true)
            .WithButton($"1 / {fields.Count}", "page", ButtonStyle.Secondary, disabled: true)
            .WithButton(">", "plugin-list-page-forward", ButtonStyle.Success, disabled: (fields.Count is 1));

        await FollowupAsync("1", embed: embed.Build(), components: builder.Build());
    }

    public async Task PLBtnExecuted(SocketMessageComponent msg, SocketUser user) {
        if (msg.User != user) { return; }
        await msg.DeferAsync();

        var content = msg.Message.CleanContent;
        var id = msg.Data.CustomId;
        var i = 0;

        if (id == "plugin-list-page-back") {
            i = Convert.ToInt32(content) - 1;
        } else if (id == "plugin-list-page-forward") {
            i = Convert.ToInt32(content) + 1;
        } else { return; }

        var embed = new EmbedBuilder()
            .WithTitle("Macro Deck Extensions")
            .WithDescription("This is the list of Macro Deck Extensions.")
            .WithFields(fields[i - 1]);

        var builder = new ComponentBuilder()
            .WithButton("<", "plugin-list-page-back", ButtonStyle.Success, disabled: (i is 1))
            .WithButton($"{i} / {fields.Count}", "page", ButtonStyle.Secondary, disabled: true)
            .WithButton(">", "plugin-list-page-forward", ButtonStyle.Success, disabled: (i == fields.Count));

        await msg.Message.ModifyAsync(msg => {
            msg.Content = $"{i}";
            msg.Embed = embed.Build();
            msg.Components = builder.Build();
        });
    }
}

public class Extension {
    public string? packageId;
    public string? extensionType;
    public string? name;
    public string? author;
    public string? githubRepository;
    public ulong? dSupportUserId;
}

public class ExtensionFile {
    public string? version;
    public int? minAPIVersion;
}