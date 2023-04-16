namespace MacroBot.Extensions;

public static class StringExtensions
{
    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Length > maxLength
            ? value[..maxLength] + truncationSuffix
            : value;
    }

    public static bool IsNullOrWhiteSpace(this string? value) {
        return String.IsNullOrWhiteSpace(value) && String.IsNullOrEmpty(value);
    }

    public static string Remove(this string value, string toRemove)
    {
        return value.Replace(toRemove, "");
    }
    
    public static bool Contains(this string source, string toCheck, StringComparison comp)
    {
        return source?.IndexOf(toCheck, comp) >= 0;
    }
}