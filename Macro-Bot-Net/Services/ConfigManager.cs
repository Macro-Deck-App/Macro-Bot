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
	}
}
