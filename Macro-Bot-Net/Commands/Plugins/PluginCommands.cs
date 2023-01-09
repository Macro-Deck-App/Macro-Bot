using Develeon64.MacroBot.Utils;
using Develeon64.MacroBot.Models;
using Develeon64.MacroBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using Develeon64.MacroBot.Commands.Tagging;
using Develeon64.MacroBot.Commands.Plugins;

namespace Develeon64.MacroBot.Commands.Plugins
{
    [Group("plugins", "Show a list of plugins, show the info of plugin, or etc.")]
    [EnabledInDm(true)]
    public class PluginCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public List<List<EmbedFieldBuilder>> fields = new();

        [SlashCommand("list", "Show a list of plugins")]
        public async Task PluginList()
        {
            fields = new();
            await DeferAsync();

            PluginUtils pluginUtils = new PluginUtils();
            Plugin[] plugins = await pluginUtils.GetPluginsAsync();

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle("Macro Deck Extensions")
                .WithDescription("This is the list of Macro Deck Extensions.");

            int f = 0;
            List<EmbedFieldBuilder> flds = new();
            foreach (Plugin plugin in plugins) {
                if (f == 15) { fields.Add(flds); flds = new(); f = 0; }
                EmbedFieldBuilder field = new();
                field.WithName($"[{plugin.type}] {plugin.package_id}");
                field.WithValue($"{((plugin.repository is not null)? $"[{plugin.name}]({plugin.repository})" : plugin.name)} by {plugin.author}\r\nLatest Version: {plugin.version}\r\nMin API Version: {plugin.target_api}");
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

            ComponentBuilder builder = new ComponentBuilder()
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
            int i = 0;

            if (id == "plugin-list-page-back") {
                i = Convert.ToInt32(content) - 1;
            } else if (id == "plugin-list-page-forward") {
                i = Convert.ToInt32(content) + 1;
            } else { return; }

            EmbedBuilder embed = new EmbedBuilder()
                .WithTitle("Macro Deck Extensions")
                .WithDescription("This is the list of Macro Deck Extensions.")
                .WithFields(fields[i - 1]);

            ComponentBuilder builder = new ComponentBuilder()
                .WithButton("<", "plugin-list-page-back", ButtonStyle.Success, disabled: (i is 1))
                .WithButton($"{i} / {fields.Count}", "page", ButtonStyle.Secondary, disabled: true)
                .WithButton(">", "plugin-list-page-forward", ButtonStyle.Success, disabled: (i == fields.Count));

            await msg.Message.ModifyAsync(msg => {
                msg.Content = $"{i}";
                msg.Embed = embed.Build();
                msg.Components = builder.Build();
            });
        }

        [SlashCommand("info", "Show the info of the specific plugin")]
        public async Task PluginInfo([Summary(description: "the name to search on")] string name)
        {
            await DeferAsync();

            PluginUtils pluginUtils = new PluginUtils();
            Plugin[] plugins = await pluginUtils.GetPluginsAsync();

            var plg = plugins[0];
            for (int i = 0; plugins.Count() > i; i++)
            {
                bool contains = plugins[i].name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0;
                bool contains2 = plugins[i].package_id.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0;
                if (contains || contains2)
                {
                    plg = plugins[i];

                    DiscordEmbedBuilder embed = new()
                    {
                        Title = plg.type + " | " + plg.package_id,
                    };

                    embed.AddField("Name", plg.name, true);
                    embed.AddField("Author", plg.author, true);
                    embed.AddField("Version", plg.version, true);

                    if (!string.IsNullOrWhiteSpace(plg.repository))
                    {
                        ComponentBuilder component = new ComponentBuilder();
                        component.WithButton("Repository", url: plg.repository, style: ButtonStyle.Link);

                        await FollowupAsync(embed: embed.Build(), components: component.Build());
                    }
                    else
                    {
                        await FollowupAsync(embed: embed.Build());
                    }

                    return;
                }
            }

            await FollowupAsync($"Can't find a plugin that contains the name {name}", ephemeral: true);
        }
    }

}
