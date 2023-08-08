using Discord;
using Discord.Interactions;

namespace MacroBot.Core.Discord.Modules.OldExtensionStore
{
    [Group("plugins", "Show a list of plugins, show the info of plugin, or etc.")]
    [EnabledInDm(true)]
    public class PluginCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("list", "Show a list of plugins")]
        public async Task PluginList()
        {
            await DeferAsync(ephemeral: true);

            PluginUtils pluginUtils = new PluginUtils();
            var embed = pluginUtils.SetPluginsAsEmbeds(await pluginUtils.GetPluginsAsync());
            List<Embed[]> embeds = embed.Chunk(10).ToList();

            foreach (Embed[] embeds1 in embeds)
            {
                await FollowupAsync(embeds: embeds1);
            }
        }
    }
}