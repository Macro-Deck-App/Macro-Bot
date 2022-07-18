using Develeon64.MacroBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace Develeon64.MacroBot.Commands {
	public class TicketActionButton : InteractionModuleBase<SocketInteractionContext> {
		[ComponentInteraction("ticket_action|close_ticket")]
		public async Task CloseTicket () {
			await (this.Context.Channel as SocketTextChannel).DeleteAsync();
			await DatabaseManager.DeleteTicket(this.Context.Channel.Id);

			await this.RespondAsync("Your ticket is now closed.", ephemeral: true);
		}

		[ComponentInteraction("ticket_action|more_help")]
		public async Task MoreHelp () {
			await this.Context.Guild.GetTextChannel(Program.globalConfig.getObject("channels").ToObject<JObject>()["supportTeamChannelID"].ToObject<ulong>()).SendMessageAsync($"{this.Context.User.Mention} ({this.Context.User.Username}) needs the help of a <@&{Program.globalConfig.getObject("roles").ToObject<JObject>()["supportRoleID"].ToObject<ulong>()}> in <#{this.Context.Channel.Id}>!");

			ActionRowComponent oldRow = (this.Context.Interaction as SocketMessageComponent).Message.Components.ElementAt(0);
			ActionRowBuilder row = new();
			foreach (ButtonComponent button in oldRow.Components) {
				ButtonBuilder builder = button.ToBuilder();
				if (button.CustomId.Contains("more_help"))
					builder.WithDisabled(true);
				row.WithButton(builder);
			}

			await (this.Context.Interaction as SocketMessageComponent).Message.ModifyAsync((message) => {
				message.Components = new ComponentBuilder().AddRow(row).Build();
			});
			await this.RespondAsync($"The <@&{Program.globalConfig.getObject("roles").ToObject<JObject>()["supportRoleID"].ToObject<ulong>()}-Team has been contacted.");
		}
	}
}
