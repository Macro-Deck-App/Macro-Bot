namespace MacroBot.Core.Extensions;

public static class StringExtensions
{
    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Length > maxLength
            ? value[..maxLength] + truncationSuffix
            : value;
    }
}