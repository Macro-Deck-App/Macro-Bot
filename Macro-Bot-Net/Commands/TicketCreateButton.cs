using Develeon64.MacroBot.Models;
using Develeon64.MacroBot.Services;
using Develeon64.MacroBot.Utils;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace Develeon64.MacroBot.Commands {
	public class TicketCreateButton : InteractionModuleBase<SocketInteractionContext> {
		[ComponentInteraction("ticket_create|no_connect")]
		public async Task CreateNoConnectTicket () {
			if (!await DatabaseManager.TicketExists(this.Context.User.Id)) {
				RestTextChannel channel = await this.Context.Guild.CreateTextChannelAsync($"ticket-{this.Context.User.Username}", (c) => {
					c.CategoryId = Program.globalConfig.getObject("channels").ToObject<JObject>()["ticketCategoryID"].ToObject<ulong>();
					List<Overwrite> overwrites = this.Context.Guild.GetCategoryChannel((ulong)c.CategoryId.Value).PermissionOverwrites.ToList();
					overwrites.Add(new Overwrite(this.Context.User.Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow)));
					c.PermissionOverwrites = overwrites;
				});
				DiscordEmbedBuilder embed = new() {
					Title = "Connection issues",
					Description = $"We are sorry, that you have problems connecting your device to the server!\nPlease make sure you read the <#{Program.globalConfig.getObject("channels").ToObject<JObject>()["faqChannelID"].ToObject<ulong>()}>.",
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

				RestMessage message = await channel.SendMessageAsync($"{this.Context.User.Mention}, your Ticket has been created: *Connection Issues *", embed: embed.Build(), components: buttons.Build());
				await DatabaseManager.CreateTicket(this.Context.User.Id, channel.Id, message.Id);

				await this.RespondAsync("Your ticket is created.", ephemeral: true);
			}
			else {
				await this.RespondAsync("There is already an open Ticket! Please use your ticket to get support.", ephemeral: true);
			}
		}

		[ComponentInteraction("ticket_create|reason")]
		public async Task CreateTicket () {
			if (!await DatabaseManager.TicketExists(this.Context.User.Id)) {
				switch ((this.Context.Interaction as SocketMessageComponent).Data.Values.ElementAt(0)) {
					case "no_connect":
						await this.CreateNoConnectTicket();
						break;
					case "plugin_problem":
					case "plugin_problems":
						ModalBuilder modal = new("Plugin-Problem", "ticket_create|plugin_problem");
						List<IMessageComponent> components = new();
						SelectMenuBuilder menu = new() {
							CustomId = "ticket_create|plugin_problem|plugin_name",
							Placeholder = "Please select the Plugin",
						};
						menu.AddOption("Twitch-Plugin", "twitch");
						menu.AddOption("Spotify-Plugin", "spotify");
						menu.AddOption("OBS-Plugin", "obs");
						menu.AddOption("Windows Utils-Plugin", "utils");
						components.Add(menu.Build());
						modal.AddComponents(components, 0);
						await this.RespondWithModalAsync(modal.Build());
						break;
					case "wrong_expectation":
					case "setup_assistance":
					default:
						break;
				}
			}
			else {
				await this.RespondAsync("There is already an open Ticket! Please use your ticket to get support.", ephemeral: true);
			}
		}
	}
}
