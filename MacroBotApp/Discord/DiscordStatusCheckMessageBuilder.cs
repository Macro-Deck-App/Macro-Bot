using Discord;
using MacroBot.Models;

namespace MacroBot.Discord;

public static class DiscordStatusCheckMessageBuilder
{
    private static readonly string[] StatusIcons = { "🟢", "🔴", "🟠" };
    private static readonly string[] StatusDescriptions = { "Online", "Offline", "Limited"};

    public static Embed Build(StatusCheckResult[] statusCheckResults)
    {
        EmbedBuilder embed = new();
        embed.WithTitle("Macro Deck Service Status");
        embed.WithDescription(statusCheckResults.Any(x => x is { Online: true, OnlineWithWarnings: false }) 
            ? "✅ All services are working."
            : "⚠ There is a problem on one or more services.");
        embed.WithColor(statusCheckResults.Any(x => x is { Online: true, OnlineWithWarnings: false }) 
            ? Color.Green 
            : Color.Gold);

        foreach (var statusCheckResult in statusCheckResults)
        {
            var status = statusCheckResult.Online
                ? statusCheckResult.OnlineWithWarnings ? 2 : 0
                : 1;
            embed.AddField(statusCheckResult.Name,
                $"Status: {StatusIcons[status]} ({StatusDescriptions[status]}, {statusCheckResult.StatusCode})");
        }
        
        embed.WithFooter("Last change");
        embed.WithCurrentTimestamp();

        return embed.Build();
    }
    
}