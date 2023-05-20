using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using MacroBot.Config;
using MacroBot.DataAccess.RepositoryInterfaces;

namespace MacroBot.Discord.Modules.Reports;

[Group("report", "Report system")]
[UsedImplicitly]
public class ReportingCommands : InteractionModuleBase<SocketInteractionContext>
{
	public static List<UserReportAssignable> createReportAssignments = new();
	private readonly BotConfig _botConfig;
	private readonly CommandsConfig _commandsConfig;
	private readonly ReportUtils _reportingUtils;
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public ReportingCommands(CommandsConfig commandsConfig, IServiceScopeFactory serviceScopeFactory,
		ReportUtils reportingUtils, BotConfig botConfig)
	{
		_commandsConfig = commandsConfig;
		_serviceScopeFactory = serviceScopeFactory;
		_reportingUtils = reportingUtils;
		_botConfig = botConfig;
	}

	// Commands
	[MessageCommand("Report this Message")]
	public async Task ReportMessage(IUserMessage message)
	{
		// Handle Assignables
		var assignable = createReportAssignments.Find(x => x.guildId == Context.Guild.Id && x.reporterId == Context.User.Id);
		if (assignable != null)
		{
			createReportAssignments.Remove(assignable);
		}

		createReportAssignments.Add(new UserReportAssignable(Context.Guild.Id, message.Author.Id, Context.User.Id, message.Id, Context.Channel.Id));
		
		await RespondWithModalAsync<ReportCreateMessageModal>("report_message_create");
	}

	[UserCommand("Report this User")]
	public async Task ReportUser(IGuildUser user)
	{
		// Handle Assignables
		var assignable = createReportAssignments.Find(x => x.guildId == Context.Guild.Id && x.reporterId == Context.User.Id);
		if (assignable != null)
		{
			createReportAssignments.Remove(assignable);
		}

		createReportAssignments.Add(new UserReportAssignable(Context.Guild.Id, user.Id, Context.User.Id));
		
		await RespondWithModalAsync<ReportCreateUserModal>("report_user_create");
	}

	[SlashCommand("list", "List reports")]
	public async Task ReportList()
	{
		await using var scope = _serviceScopeFactory.CreateAsyncScope();
		var reportRepository = scope.ServiceProvider.GetRequiredService<IReportRepository>();
		var r = await reportRepository.GetReportsForGuild(Context.Guild.Id);
		var reports = r.ToArray();

		// Haven't found a way to efficiently put 100s of reports in a single message, so here we go!
		var str = reports.Aggregate("",
			(current, report) =>
				current +
				$"[{report.Id}] > {report.User} by {report.Reporter} {(report.Message is not null ? $"on {report.Channel}/{report.Message}" : "")}\r\n");

		await RespondAsync($"This is the reports!\r\n\r\n```{str}```", ephemeral: true);
	}

	[SlashCommand("get", "Get a report")]
	public async Task Get(string id)
	{
		var guild = Context.Guild;
		await using var scope = _serviceScopeFactory.CreateAsyncScope();
		var reportRepository = scope.ServiceProvider.GetRequiredService<IReportRepository>();
		var report = await reportRepository.GetReport(id);

		if (report is null)
		{
			var embedBuildern = new EmbedBuilder
			{
				Title = String.Format("Report {0} does not exist!", id)
			};
			await RespondAsync(embed: embedBuildern.Build(), ephemeral: true);
			return;
		}

		var componentBuilder = new ComponentBuilder();
		var embedBuilder = new EmbedBuilder
		{
			Title = $"Report {id}",
			Timestamp = new DateTimeOffset(report.Reported),
			ThumbnailUrl = guild.GetUser(report.User).GetDisplayAvatarUrl()
		};
		embedBuilder.AddField("Reported by", guild.GetUser(report.Reporter).Mention, true);
		embedBuilder.AddField("User Reported", guild.GetUser(report.User).Mention, true);
		if (report.Channel != null && report.Message != null)
		{
			var msg = await guild.GetTextChannel((ulong)report.Channel).GetMessageAsync((ulong)report.Message);
			embedBuilder.AddField("Content of the message reported", msg.Content);
			componentBuilder.WithButton("Jump to message", style: ButtonStyle.Link, url: msg.GetJumpUrl());
		}
		embedBuilder.AddField("Reason", report.Content);

		await RespondAsync(embed: embedBuilder.Build(), components: componentBuilder.Build(), ephemeral: true);
	}
}