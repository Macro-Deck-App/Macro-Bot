using Develeon64.MacroBot.Models;
using Newtonsoft.Json;

namespace Develeon64.MacroBot.Services {
	public static class ConfigManager {
		public static AppConfig GlobalConfig {
			get {
				if (CacheManager.Get<AppConfig>("config") is not AppConfig config) {
					config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText("Config/global.json"));
					CacheManager.Set("config", new CacheObject<AppConfig>(config));
				}
				return config;
			}
		}

		public static CommandsConfig CommandsConfig
		{
			get
			{
				if (CacheManager.Get<CommandsConfig>("commandsConfig") is not CommandsConfig config)
				{
					config = JsonConvert.DeserializeObject<CommandsConfig>(File.ReadAllText("Config/commands.json"));
					CacheManager.Set("commandsConfig", new CacheObject<CommandsConfig>(config));
				}
				return config;
			}
		}
	}
}
