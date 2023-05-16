namespace MacroBot.Extensions;

public static class ArrayExtensions
{
    public static string Join<T>(this IEnumerable<T> enumerable, string separator, string? lastSeparator = null)
    {
        var array = enumerable as T[] ?? enumerable.ToArray();
        if (!string.IsNullOrEmpty(lastSeparator) && array.Length > 1)
            return string.Join(separator, array.Take(array.Length - 1)) + $"{lastSeparator}" + array.LastOrDefault();

        return string.Join(separator, array);
    }
}