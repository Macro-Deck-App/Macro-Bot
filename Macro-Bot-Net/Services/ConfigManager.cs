using Develeon64.MacroBot.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Develeon64.MacroBot.Services
{
    public class ConfigManager
    {
        private string configName;

        public ConfigManager(string configName)
        {
            this.configName = configName;
        }

        public JObject getObject(string objectIndex)
        {
            JObject config = readFile();

            return null;
        }
        public JObject setObject(string objectIndex, JObject obj)
        {
            return null;
        }

        private JObject readFile()
        {
            try
            {
                //String fileContent = File.ReadAllText();
                Logger.Info(Modules.Config, Process.GetCurrentProcess().MainModule.FileName);

            } catch (Exception ex)
            {
                Logger.Critical(Modules.Config, "Failed to read config file \"" + configName + ".json" + "\"\n" + ex.Message);
            }

            return null;
        }
    }
}
