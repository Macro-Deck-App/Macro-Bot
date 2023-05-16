using System.ComponentModel.Design;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;
using MacroBot.Config;
using MacroBot.DataAccess.Entities;
using MacroBot.DataAccess.RepositoryInterfaces;
using MacroBot.Extensions;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroBot.Discord.Modules.Reports;

public class ReportingInteractions : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger _logger = Log.ForContext<ReportingInteractions>();
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ReportUtils _reportUtils;
    private readonly BotConfig _botConfig;

    public ReportingInteractions(IServiceScopeFactory serviceScopeFactory, ReportUtils reportUtils, BotConfig botConfig)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _reportUtils = reportUtils;
        _botConfig = botConfig;
    }

    public async Task GenerateReportMessage(Report report)
    {
        try
        {
            await Context.Guild.GetTextChannel(_botConfig.Channels.ReportsChannelId).SendMessageAsync(
                embed: await ReportUtils.GenerateReportEmbed(report, Context.Guild),
                components: await ReportUtils.GenerateReportComponent(report, Context.Guild)
            );
        }
        catch (Exception e)
        {
            _logger.Error(e, "Cannot send report message!");
        }
    }

    [ComponentInteraction("report-select-action")]
    public async Task HandleReportAction(string[] actions)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var reportRepository = scope.ServiceProvider.GetRequiredService<IReportRepository>();
        
        foreach (string action in actions)
        {
            var splAct = action.Split('.');
            var report = await reportRepository.GetReport(splAct[0]);

            switch (splAct[1])
            {
                case "deletemsg":
                    var msg = await Context.Client.GetGuild(report.Guild).GetTextChannel((ulong)report.Channel)
                          .GetMessageAsync((ulong)report.Message);
                    await msg.DeleteAsync();
                    break;
                case "timeout":
                    await Context.Client.GetGuild(report.Guild).GetUser(report.User).SetTimeOutAsync(TimeSpan.FromMinutes(splAct[2].ToInt32()));
                    break;
                case "kick":
                    await Context.Client.GetGuild(report.Guild).GetUser(report.User).KickAsync();
                    break;
                case "ban":
                    await Context.Client.GetGuild(report.Guild).GetUser(report.User).BanAsync();
                    break;
            }

            await RespondAsync("Report taken action successfully.", ephemeral: true);
        }
    }

    [ModalInteraction("report_user_create")]
    public async Task HandleCreateUserReportModal(ReportCreateUserModal modal)
    {
        var assignable =
            ReportingCommands.createReportAssignments.Find(x =>
                x.guildId == Context.Guild.Id && x.reporterId == Context.User.Id);

        if (assignable is not null)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var reportRepository = scope.ServiceProvider.GetRequiredService<IReportRepository>();
            var reportId = await reportRepository.CreateReport(assignable.reporterId, assignable.userId,
                assignable.guildId, modal.ReportReason);

            var embedBuilder = new EmbedBuilder
            {
                Title = "User Reported",
            };
            embedBuilder.AddField("Report ID (keep it handy!)", reportId);
            embedBuilder.AddField("Reason", modal.ReportReason);
            embedBuilder.WithColor(new Color(50, 255, 50));

            ReportingCommands.createReportAssignments.Remove(assignable);
            
            await GenerateReportMessage(await reportRepository.GetReport(reportId));
            await RespondAsync( ephemeral:true, embed: embedBuilder.Build());
        }
        else
        {
            _logger.Error(
                $"Can't get Report info for user {Context.User.Username} ({Context.User.Id}) in {Context.Guild.Name}!");
            await RespondAsync(_reportUtils.getReportInfoError(), ephemeral: true);
        }
    }

    [ModalInteraction("report_message_create")]
    public async Task HandleCreateMessageReportModal(ReportCreateMessageModal modal)
    {
        var assignable =
            ReportingCommands.createReportAssignments.Find(x =>
                x.guildId == Context.Guild.Id && x.reporterId == Context.User.Id);

        if (assignable is not null)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var reportRepository = scope.ServiceProvider.GetRequiredService<IReportRepository>();
            var reportId = await reportRepository.CreateReport(assignable.reporterId, assignable.userId,
                assignable.guildId, modal.ReportReason, (UInt64)assignable.channelId, (ulong)assignable.messageId);

            var embedBuilder = new EmbedBuilder
            {
                Title = "Message Reported",
            };
            embedBuilder.AddField("Report ID (keep it handy!)", reportId);
            embedBuilder.AddField("Reason", modal.ReportReason);
            embedBuilder.WithColor(new Color(50, 255, 50));

            ReportingCommands.createReportAssignments.Remove(assignable);

            await GenerateReportMessage(await reportRepository.GetReport(reportId));
            await RespondAsync( ephemeral:true, embed: embedBuilder.Build());
        }
        else
        {
            _logger.Error(
                $"Can't get Report info for user {Context.User.Username} ({Context.User.Id}) in {Context.Guild.Name}!");
            await RespondAsync(_reportUtils.getReportInfoError(), ephemeral: true);
        }
    }
}

public class ReportCreateMessageModal : IModal
{
    [InputLabel("Report Reason")]
    [ModalTextInput("report_reason", TextInputStyle.Paragraph, minLength: 1, maxLength: 4000)]
    public string ReportReason { get; set; }

    public string Title => "Report a message";
}

public class ReportCreateUserModal : IModal
{
    [InputLabel("Report Reason")]
    [ModalTextInput("report_reason", TextInputStyle.Paragraph, minLength: 1, maxLength: 4000)]
    public string ReportReason { get; set; }

    public string Title => "Report a user";
}

public class ReportApproveModal : IModal
{
    [InputLabel("Approve Reason")]
    [ModalTextInput("report_reason", TextInputStyle.Paragraph, minLength: 1, maxLength: 4000)]
    public string ReportReason { get; set; }

    public string Title => "Approve Report";
}

public class ReportDeclineModal : IModal
{
    [InputLabel("Decline Reason")]
    [ModalTextInput("report_reason", TextInputStyle.Paragraph, minLength: 1, maxLength: 4000)]
    public string ReportReason { get; set; }

    public string Title => "Decline Report";
}