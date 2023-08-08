using System.Net.Http.Json;
using Discord;

namespace MacroBot.Core.Discord.Modules.OldExtensionStore
{
    public class PluginUtils
    {
        public async Task<List<Plugin>> GetPluginsAsync()
        {
            using var httpClient = new HttpClient();
            var json = await httpClient.GetFromJsonAsync<List<Plugin>>("https://macrodeck.org/extensionstore/extensionstore.php?action=list&includeicons=false");
            return json;
        }

        public List<Embed> SetPluginsAsEmbeds(List<Plugin> plugins)
        {
            List<Embed> embeds = new();
            EmbedBuilder embedBuilder = new();

            int pl = 0;
            foreach (Plugin plugin in plugins)
            {
                if (pl >= 14)
                {
                    embedBuilder.WithTitle("Macro Deck Plugins and Icon Packs");
                    embedBuilder.WithFooter($"Page {embeds.Count() + 1}");
                    embeds.Add(embedBuilder.Build());
                    embedBuilder = new();
                    pl = 0;
                }
                embedBuilder.AddField($"[{plugin.Type}] {plugin.PackageId}", $"{(plugin.Repository is not null
                    ? $"[{plugin.Name}]({plugin.Repository})"
                    : $"{plugin.Name}")} by {plugin.Author}", true);
                pl++;
            }

            return embeds;
        }


    }
}
