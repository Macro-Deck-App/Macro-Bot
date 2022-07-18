using Develeon64.MacroBot.Utils;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Develeon64.MacroBot.Commands {
	public class SendTicketMessageCommand : InteractionModuleBase<SocketInteractionContext> {
		[SlashCommand("sendticketmessage", "Send the base message into the Ticket-Channel")]
		public async Task SendTicketMessage () {
			DiscordEmbedBuilder embed = new() {
				Title = "__Ticket-Support__",
				Description = "Click on the buttons below to create a ticket.\nPlease select the appropriate topic.",
			};
			ComponentBuilder buttons = new();
			buttons.AddRow(new ActionRowBuilder().WithButton("Can't connect", "ticket_create|no_connect"));
			buttons.AddRow(new ActionRowBuilder().WithButton("Something is wrong", "ticket_create|wrong_expectation"));
			buttons.AddRow(new ActionRowBuilder().WithButton("Setup-Assistance", "ticket_create|setup_assistance"));
			buttons.AddRow(new ActionRowBuilder().WithButton("Other", "ticket_create|other"));

			SocketTextChannel channel = this.Context.Guild.GetTextChannel(998293017557487697);
			await channel.SendMessageAsync(embed: embed.Build(), components: buttons.Build());
			await this.RespondAsync($"Message successfully sent to <#{channel.Id}>", ephemeral: true);
		}
	}
}
