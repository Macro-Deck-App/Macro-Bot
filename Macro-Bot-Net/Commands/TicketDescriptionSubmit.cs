using Develeon64.MacroBot.Logging;
using Develeon64.MacroBot.Services;
using Develeon64.MacroBot.Utils;
using Discord;
using Discord.Interactions;
using Newtonsoft.Json.Linq;

namespace Develeon64.MacroBot.Commands {
	public class TicketDescriptionSubmit : InteractionModuleBase<SocketInteractionContext> {
		[ModalInteraction("ticket_create|wrong_expectation|reason")]
		public async Task CreateExpectation (Modal modal) {
			DiscordEmbedBuilder embed = await TicketCreateButton.CreateEmbed(this.Context.Client);
			embed.WithTitle("Expected something different");
			embed.WithDescription("We're sorry, that you expected something different from Macro Deck.");
			embed.AddField("Your Description", this.Context.Interaction.Data, false);

			ComponentBuilder buttons = new();
			buttons.AddRow(await TicketCreateButton.CreateActionRow());

			await TicketCreateButton.CreateChannelAsync(this.Context.Guild, this.Context.User, $"{this.Context.User.Mention} your ticket is created here and a <@&{ConfigManager.GlobalConfig.Roles.SupportRoleId}> will help you soon:", embed.Build(), buttons.Build());
			await this.RespondAsync("Your Ticket is created!", ephemeral: true);
		}

		[ModalInteraction("ticket_create|other|reason")]
		public async Task CreateOther (Modal modal) {
			Logger.Debug(Modules.Tickets, "Modal received!");
			DiscordEmbedBuilder embed = await TicketCreateButton.CreateEmbed(this.Context.Client);
			embed.WithTitle("Some other problems");
			embed.WithDescription("We're sorry, that you are facing problems with Macro Deck.");
			embed.AddField("Your Description", this.Context.Interaction.Data, false);

			ComponentBuilder buttons = new();
			buttons.AddRow(await TicketCreateButton.CreateActionRow());

			await TicketCreateButton.CreateChannelAsync(this.Context.Guild, this.Context.User, $"{this.Context.User.Mention} your ticket is created here and a <@&{ConfigManager.GlobalConfig.Roles.SupportRoleId}> will help you soon:", embed.Build(), buttons.Build());
			await this.RespondAsync("Your Ticket is created!", ephemeral: true);
		}
	}
}
