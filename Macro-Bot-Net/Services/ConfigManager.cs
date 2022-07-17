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
        private static string dirPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "/config";
        private string configName;
        private string filePath;

        public ConfigManager(string configName)
        {
            this.configName = configName.ToLower();
            this.filePath = dirPath + "/" + configName + ".json";

            setupStructure();
        }

        public JToken getObject(string objectIndex)
        {
            JObject config = readFile();
            if (config != null)
            {
                return config[objectIndex];
            }
            return null;
        }
        public void setObject(string objectIndex, JToken obj)
        {
            JObject config = readFile();
            if (config != null)
            {
                config[objectIndex] = obj;

                writeFile(config);
            }
        }
        public void removeObject(string objectIndex)
        {
            JObject config = readFile();
            if (config != null)
            {
                config.Remove(objectIndex);

                writeFile(config);
            }
        }

        private JObject readFile()
        {
            try
            {
                setupStructure();

                String fileContent = File.ReadAllText(filePath);
                JObject config = JObject.Parse(fileContent);

                return config;

            }
            catch (Exception ex)
            {
                Logger.Critical(Modules.Config, "Failed to read config file \"" + configName + ".json" + "\"\n" + ex.Message);
            }

            return null;
        }
        private void writeFile(JObject config)
        {
            try
            {
                File.WriteAllText(filePath ,config.ToString());

            } catch (Exception ex)
            {
                Logger.Critical(Modules.Config, "Failed to save config file \"" + configName + ".json" + "\"\n" + ex.Message);
            }
        }

        private void setupStructure()
        {
            Directory.CreateDirectory(dirPath);
            if (!File.Exists(filePath))
            {
                File.AppendAllText(filePath, "{}");
            }
        }
    }
}
