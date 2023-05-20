using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MacroBot.DataAccess.RepositoryInterfaces;

namespace MacroBot.Discord.Modules.Reports;

public class ReportUtils
{
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public ReportUtils(IServiceScopeFactory serviceScopeFactory)
	{
		_serviceScopeFactory = serviceScopeFactory;
	}

	public async Task AutoCompleteReport(SocketInteractionContext context)
	{
		var userInput = (context.Interaction as SocketAutocompleteInteraction)?.Data.Current.Value.ToString();

		await using var scope = _serviceScopeFactory.CreateAsyncScope();
		var reportRepository = scope.ServiceProvider.GetRequiredService<IReportRepository>();
		List<AutocompleteResult> resultList = (from tag in await reportRepository.GetReportsForGuild(context.Guild.Id)
			select new AutocompleteResult($"{tag.User} reported by {tag.Reporter}", tag.Id)).ToList();

		var results = resultList.AsEnumerable()
			.Where(x => x.Name.StartsWith(userInput, StringComparison.InvariantCultureIgnoreCase));

		await ((SocketAutocompleteInteraction)context.Interaction).RespondAsync(results.Take(25));
	}

	public static async Task<Embed> GenerateReportEmbed(Report report, SocketGuild guild)
	{
		var embedBuilder = new EmbedBuilder
		{
			Title = $"Report {report.Id}",
			Timestamp = new DateTimeOffset(report.Reported),
			ThumbnailUrl = guild.GetUser(report.User).GetDisplayAvatarUrl()
		};
		embedBuilder.AddField("Reported by", guild.GetUser(report.Reporter).Mention, true);
		embedBuilder.AddField("User Reported", guild.GetUser(report.User).Mention, true);
		if (report.Channel != null && report.Message != null)
		{
			var msg = await guild.GetTextChannel((ulong)report.Channel).GetMessageAsync((ulong)report.Message);
			embedBuilder.AddField("Content of the message reported", msg.Content);
		}
		embedBuilder.AddField("Reason", report.Content);

		return embedBuilder.Build();
	}

	public static async Task<MessageComponent> GenerateReportComponent(Report report, SocketGuild guild)
	{
		var componentBuilder = new ComponentBuilder();
		var selectMenuBuilder = new SelectMenuBuilder()
		{
			Placeholder = "Select an action...",
			MaxValues = 2,
			MinValues = 1,
			CustomId = "report-select-action"
		};
		selectMenuBuilder.AddOption("Decline report", $"{report.Id}.decline");

		if (report.Channel != null && report.Message != null)
		{
			var msg = await guild.GetTextChannel((ulong)report.Channel).GetMessageAsync((ulong)report.Message);
			componentBuilder.WithButton("Jump to message", style: ButtonStyle.Link, url: msg.GetJumpUrl());
			selectMenuBuilder.AddOption("Delete message", $"{report.Id}.deletemsg");
		}

		selectMenuBuilder.AddOption("Timeout user for 30 minutes", $"{report.Id}.timeout.30");
		selectMenuBuilder.AddOption("Timeout user for 3 hours", $"{report.Id}.timeout.180");
		selectMenuBuilder.AddOption("Timeout user for 1 day", $"{report.Id}.timeout.1440");
		selectMenuBuilder.AddOption("Kick user", $"{report.Id}.kick");
		selectMenuBuilder.AddOption("Ban user", $"{report.Id}.ban");

		componentBuilder.WithSelectMenu(selectMenuBuilder);
		return componentBuilder.Build();
	}

	public Embed buildAlreadyExistsError(string reportId)
	{
		var embedBuilder = new EmbedBuilder
		{
			Title = "Report already exists",
			Description = $"The report `{reportId}` already exists!"
		};
		embedBuilder.WithColor(new Color(255, 50, 50));

		return embedBuilder.Build();
	}

	public Embed buildTagNotFoundError(string reportId)
	{
		var embedBuilder = new EmbedBuilder
		{
			Title = "Report not found",
			Description = $"The report `{reportId}` could not be found in the database!"
		};
		embedBuilder.WithColor(new Color(255, 50, 50));

		return embedBuilder.Build();
	}

	public string getReportInfoError()
	{
		return "An internal error occured while getting Report information, please try again.";
	}
}

public class UserReportAssignable
{
	public UserReportAssignable(ulong guildID, ulong userID, ulong reporterID, ulong? messageID = null, ulong? channelID = null)
	{
		userId = userID;
		guildId = guildID;
		reporterId = reporterID;
		messageId = messageID;
		channelId = channelID;
	}
	// Apparently i have not found a more efficient way in asigning the report id to an discord modal so i needed to temporarly save this assignment into a List

	public ulong userId { get; private set; }
	public ulong guildId { get; private set; }
	public ulong reporterId { get; private set; }
	public ulong? messageId { get; private set; }
	public ulong? channelId { get; private set; }
}