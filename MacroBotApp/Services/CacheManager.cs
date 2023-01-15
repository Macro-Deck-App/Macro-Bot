using MacroBot.Models;

namespace MacroBot.Services;

public static class CacheManager {
	private static Dictionary<string, CacheObject<object>> cache = new();

	public static T? Get<T> (string key) {
		return (T?)cache.GetValueOrDefault(key)?.GetValue();
	}

	public static void Set<T> (string key, CacheObject<T> value) {
		try {
			cache.Add(key, new CacheObject<object>(value.GetValue()));
		} catch (Exception) {}
	}
}