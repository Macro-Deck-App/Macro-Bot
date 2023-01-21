using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord;

namespace MacroBot.Discord.Modules.OldExtensionStore
{
    public class PluginUtils
    {
        public async Task<Plugin[]> GetPluginsAsync()
        {
            HttpClient httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync("https://macrodeck.org/extensionstore/extensionstore.php?action=list&includeicons=false");
            Plugin[] plugins = JsonConvert.DeserializeObject<Plugin[]>(json);
            return plugins;
        }

        public List<Embed> SetPluginsAsEmbeds(Plugin[] plugins) {
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
                embedBuilder.AddField($"[{plugin.type}] {plugin.package_id}", $"{(plugin.repository is not null
                    ? $"[{plugin.name}]({plugin.repository})"
                    : $"{plugin.name}")} by {plugin.author}", true);
                pl++;
            }

            return embeds;
        }


    }
}
