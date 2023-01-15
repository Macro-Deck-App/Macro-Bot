namespace MacroBot.Models;

public class CacheObject<T> {
	private T data;
	private DateTime time;

	public CacheObject (T data) {
		this.data = data;
		time = DateTime.Now;
	}

	public T? GetValue () {
		return DateTime.Now.Subtract(time).TotalMinutes <= 5 ? data : default;
	}

	public Type Type {
		get {
			return typeof(T);
		}
	}
}