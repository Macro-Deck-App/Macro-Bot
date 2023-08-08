using Discord;

namespace MacroBot.Core.Extensions;

public static class EmbedBuilderExtensions
{
    public static EmbedBuilder AddBlankField(this EmbedBuilder embedBuilder, bool inline = false)
    {
        return embedBuilder.AddField("\u200b", "\u200b", inline);
    }
}