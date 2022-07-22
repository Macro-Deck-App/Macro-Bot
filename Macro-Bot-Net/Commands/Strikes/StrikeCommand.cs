using Develeon64.MacroBot.Models;
using Develeon64.MacroBot.Services;
using Develeon64.MacroBot.Utils;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Develeon64.MacroBot.Commands.Strikes {
	[Group("strike", "Strike-System")]
	public class StrikeCommand : InteractionModuleBase<SocketInteractionContext> {
		[SlashCommand("add", "Give an User a Strike")]
		public async Task AddStrike ([Summary(description: "The user to strike")] SocketGuildUser member, [Summary(description: "The reason for the strike")] string reason) {
			await DatabaseManager.AddStrike(member.Id, this.Context.User.Id, reason);
			int active = await DatabaseManager.GetActiveStrikes(member.Id);
			int count = await DatabaseManager.GetStrikeCount(member.Id);

			try {
				if (count > 5)
					await member.BanAsync(1, $"User has reached more than 5 Strikes in the last 90 days.\nBy: {this.Context.User.Username}\n{reason}");
				else if (count >= 5)
					await member.KickAsync($"User has reached 5 Strikes in the last 90 days.\nBy: {this.Context.User.Username}\n{reason}");
				else if (count >= 4)
					await member.SetTimeOutAsync(new TimeSpan(3, 0, 0, 0, 0), new RequestOptions() { AuditLogReason = $"User has reached 4 Strikes in the last 90 days.\nBy: {this.Context.User.Username}\n{reason}" });
				else if (count >= 3)
					await member.SetTimeOutAsync(new TimeSpan(1, 0, 0, 0, 0), new RequestOptions() { AuditLogReason = $"User has reached 3 Strikes in the last 90 days.\nBy: {this.Context.User.Username}\n{reason}" });
			}
			catch (Exception ex) {
				Console.WriteLine(ex.Message);
			}

			DiscordEmbedBuilder embed = new() {
				Title = $"__Strikes for {member.Nickname ?? member.Username}__",
				Description = $"The member now has {active}/{count} Strikes!",
				Color = new Color(191, 63, 127),
			};
			embed.WithAuthor(member);
			embed.AddField("__Reason:__", reason);

			await this.RespondAsync($"Strike for {member.Mention} has been registered!", embed: embed.Build());
		}

		[SlashCommand("remove", "Remove a Strike from a User")]
		public async Task RemoveStrike ([Summary(description: "A User from which all Strikes will be removed")] SocketGuildUser? member, [Summary(description: "A Strike-ID from the database")] int? id) {
			if (id != null) {
				await DatabaseManager.RemoveStrike(id ?? -1);
			}
			if (member != null) {
				await DatabaseManager.RemoveStrikes(member.Id);
			}
			int active = await DatabaseManager.GetActiveStrikes(member.Id);
			int count = await DatabaseManager.GetStrikeCount(member.Id);

			DiscordEmbedBuilder embed = new() {
				Title = $"__Strikes for {member.Nickname ?? member.Username}__",
				Description = $"The member now has {active}/{count} Strikes!",
				Color = new Color(127, 63, 191),
			};
			embed.WithAuthor(member);

			await this.RespondAsync($"Strike for {member.Mention} has been registered!", embed: embed.Build(), ephemeral: true);
		}
	}
}
