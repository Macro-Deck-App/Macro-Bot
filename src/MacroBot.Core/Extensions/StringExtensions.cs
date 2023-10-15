namespace MacroBot.Core.Extensions;

public static class StringExtensions
{
    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Length > maxLength
            ? value[..maxLength] + truncationSuffix
            : value;
    }

    public static string Remove(this string source, string value)
    {
        return source.Replace(value, string.Empty);
    }
}