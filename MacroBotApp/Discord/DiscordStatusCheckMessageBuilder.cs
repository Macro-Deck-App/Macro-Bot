using Discord;
using MacroBot.Extensions;
using MacroBot.Models.Status;

namespace MacroBot.Discord;

public static class DiscordStatusCheckMessageBuilder
{
	private static readonly string[] StatusIcons = { "🟢", "🔴", "🟠" };
	private static readonly Color[] StatusColors = { Color.Green, Color.Red, Color.Gold };
	private static readonly string[] StatusDescriptions = { "**Online**", "**Offline**", "**Limited**" };

	public static Embed Build(StatusCheckResult[] statusCheckResults)
	{
		EmbedBuilder embed = new();
		embed.WithTitle("Macro Deck Service Status Update");

		var nowOffline = statusCheckResults
			.Where(x => x.StateChanged && x is { Online: false, OnlineWithWarnings: false }).ToArray();
		var nowOnline = statusCheckResults
			.Where(x => x.StateChanged && x is { Online: true, OnlineWithWarnings: false }).ToArray();
		var nowOnlineWithWarnings = statusCheckResults
			.Where(x => x.StateChanged && x is { Online: true, OnlineWithWarnings: true }).ToArray();

		var description = string.Empty;
		if (nowOffline.Length > 0)
		{
			description +=
				$"‼️ {nowOffline.Select(x => $"**{x.Name}**").Join(", ", " and ")} {(nowOffline.Length == 1 ? "is" : "are")} now offline\r\n";
		}

		if (nowOnline.Length > 0)
		{
			description +=
				$"✅️ {nowOnline.Select(x => $"**{x.Name}**").Join(", ", " and ")} {(nowOnline.Length == 1 ? "is" : "are")} now back online\r\n";
		}

		if (nowOnlineWithWarnings.Length > 0)
		{
			description +=
				$"⚠️ {nowOnlineWithWarnings.Select(x => $"**{x.Name}**").Join(", ", " and ")} may not work as expected\r\n";
		}

		embed.WithDescription(description);

		embed.WithColor(statusCheckResults.All(x => x is { Online: true, OnlineWithWarnings: false })
			? Color.Green
			: Color.Gold);

		foreach (var statusCheckResult in statusCheckResults)
		{
			var status = statusCheckResult.Online
				? statusCheckResult.OnlineWithWarnings ? 2 : 0
				: 1;


			DateTimeOffset? stateChangedOffset = statusCheckResult.StateChangedAt;
			if ((!statusCheckResult.Online || statusCheckResult.OnlineWithWarnings) && stateChangedOffset.HasValue)
			{
				embed.AddField(statusCheckResult.Name,
					$"Status: {StatusIcons[status]} - {StatusDescriptions[status]} <t:{stateChangedOffset.Value.ToUnixTimeSeconds()}:R>");
			}
			else
			{
				embed.AddField(statusCheckResult.Name,
					$"Status: {StatusIcons[status]} - {StatusDescriptions[status]}");
			}
		}

		return embed.Build();
	}
}