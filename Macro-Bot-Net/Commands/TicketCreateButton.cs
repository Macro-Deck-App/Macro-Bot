using Develeon64.MacroBot.Utils;
using Discord;
using Discord.Interactions;
using Discord.Rest;

namespace Develeon64.MacroBot.Commands {
	public class TicketCreateButton : InteractionModuleBase<SocketInteractionContext> {
		[ComponentInteraction("ticket_create|no_connect")]
		public async Task CreateTicket () {
			RestTextChannel channel = await this.Context.Guild.CreateTextChannelAsync($"ticket-{this.Context.User.Username}", (c) => { c.CategoryId = 963558050596282378; });
			DiscordEmbedBuilder embed = new() {
				Title = "Connection issues",
				Description = "We are sorry, that you have problems connecting your device to the server!\nPlease make sure you read the <#997443608636563577>.",
			};
			embed.AddField("Step 1", "Do step 1", true);
			embed.AddField("Step 2", "Do step 2", true);
			embed.AddField("Step 3", "Do step 3", true);
			embed.AddField("Step 4", "Do step 4", true);
			embed.AddField("Step 5", "Do step 5", true);
			embed.AddBlankField(true);
			embed.AddBlankField(false);
			embed.AddField("Did this information help you?", "Please close the ticket, if your problem is solved.\nAfter you didn't reply within 30 days, your ticket will automatically be closed.", false);

			ComponentBuilder buttons = new();
			ActionRowBuilder row = new();
			row.WithButton("Close Ticket", "ticket_action|close_ticket");
			row.WithButton("Need more help", "ticket_action|more_help", ButtonStyle.Secondary);
			buttons.AddRow(row);

			await channel.SendMessageAsync($"{this.Context.User.Mention}, your Ticket has been created: *Connection Issues *", embed: embed.Build(), components: buttons.Build());
			await this.RespondAsync("Your ticket was created.", ephemeral: true);
		}
	}
}
