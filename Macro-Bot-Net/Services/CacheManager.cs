using Develeon64.MacroBot.Models;

namespace Develeon64.MacroBot.Services {
	public static class CacheManager {
		private static Dictionary<string, CacheObject<object>> cache = new();

		public static T? Get<T> (string key) {
			return (T?)cache.GetValueOrDefault(key)?.GetValue();
		}

		public static void Set<T> (string key, CacheObject<T> value) {
			cache.Add(key, new CacheObject<object>(value.GetValue()));
		}
	}
}
