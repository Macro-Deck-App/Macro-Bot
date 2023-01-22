using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Discord;

namespace MacroBot.Discord.Modules.OldExtensionStore
{
    public class PluginUtils
    {
        public async Task<List<Plugin>> GetPluginsAsync()
        {
            using (var httpClient = new HttpClient()) {
                var json = await httpClient.GetFromJsonAsync<List<Plugin>>("https://macrodeck.org/extensionstore/extensionstore.php?action=list&includeicons=false");
                return json;
            }
        }

        public List<Embed> SetPluginsAsEmbeds(List<Plugin> plugins) {
            List<Embed> embeds = new();
            EmbedBuilder embedBuilder = new();

            int pl = 0;
            foreach (Plugin plugin in plugins) {
                if (pl >= 14) {
                    embedBuilder.WithTitle("Macro Deck Plugins and Icon Packs");
                    embedBuilder.WithFooter($"Page {embeds.Count() + 1}");
                    embeds.Add(embedBuilder.Build());
                    embedBuilder = new();
                    pl = 0;
                }
                embedBuilder.AddField($"[{plugin.Type}] {plugin.PackageID}", $"{(plugin.Repository is not null
                    ? $"[{plugin.Name}]({plugin.Repository})"
                    : $"{plugin.Name}")} by {plugin.Author}", true);
                pl++;
            }

            return embeds;
        }


    }
}
