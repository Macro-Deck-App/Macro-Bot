using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Develeon64.MacroBot.Commands.Plugins
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
    }

    public class Plugin
    {
        [JsonProperty(PropertyName = "package-id")]
        public string package_id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public string author { get; set; }
        [JsonProperty(PropertyName = "author-discord")]
        public string author_discord { get; set; }
        public string repository { get; set; }
        [JsonProperty(PropertyName = "target-api")]
        public string target_api { get; set; }
    }
}
