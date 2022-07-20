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
		[ComponentInteraction("ticket_create|reason")]
		public async Task CreateTicket () {
			if (!await DatabaseManager.TicketExists(this.Context.User.Id)) {
				DiscordEmbedBuilder embed = await TicketCreateButton.CreateEmbed(this.Context.Client);
				ComponentBuilder buttons = new();
				ModalBuilder modal = null;

				switch ((this.Context.Interaction as SocketMessageComponent).Data.Values.ElementAt(0)) {
					case "no_connect":
						embed.WithTitle("Connection issues");
						embed.WithDescription($"We are sorry, that you have problems connecting your device to the server!\nPlease make sure you read the <#{Program.globalConfig.getObject("channels").ToObject<JObject>()["faqChannelID"].ToObject<ulong>()}>.");
						embed.AddField("Step 1", "Do step 1", true);
						embed.AddField("Step 2", "Do step 2", true);
						embed.AddField("Step 3", "Do step 3", true);
						embed.AddField("Step 4", "Do step 4", true);
						embed.AddField("Step 5", "Do step 5", true);
						embed.AddBlankField(true);
						break;
					case "setup_assistance":
						embed.WithTitle("Setup Assistance");
						embed.WithDescription($"We are sorry, that you're having problems setting your Macro Deck up. Please also see the <#{Program.globalConfig.getObject("channels").ToObject<JObject>()["faqChannelID"].ToObject<ulong>()}>-Channel and follow the below instructions.");
						embed.AddField("Step 1", "Do step 1", true);
						embed.AddField("Step 2", "Do step 2", true);
						embed.AddField("Step 3", "Do step 3", true);
						embed.AddField("Step 4", "Do step 4", true);
						embed.AddField("Step 5", "Do step 5", true);
						embed.AddField("Step 6", "Do step 6", true);
						embed.AddField("Step 7", "Do step 7", true);
						embed.AddBlankField(true);
						embed.AddBlankField(true);
						break;
					case "plugin_problem":
					case "plugin_problems":
						embed.WithTitle("Plugin problems");
						embed.WithDescription($"We are sorry, that you have problems using a Plugin.\nPlease choose the Plugin below you're having problems with to ping the Creator of the Plugin.");

						SelectMenuBuilder menu = new() {
							Placeholder = "Please select the Plugin you're having problems with:",
							CustomId = "ticket_action|plugin_select",
						};
						menu.AddOption("SuchByte.TwitchPlugin", "suchbyte.twitchplugin");
						menu.AddOption("Develeon64.SpotifyPlugin", "develeon64.spotifyplugin");
						menu.AddOption("Xenox003.MagicHome", "xenox003.magichome");
						buttons.AddRow(new ActionRowBuilder().WithSelectMenu(menu));
						break;
					case "wrong_expectation":
						modal = new() {
							Title = "Something is wrong",
							CustomId = "ticket_create|wrong_expectation|reason",
						};
						modal.AddTextInput("Description", "ticket_create|wrong_expectation|description", TextInputStyle.Paragraph, "Please describe, what's wrong in your opinion:", 2, 200, true);
						await this.RespondWithModalAsync(modal.Build());
						break;
					default:
						modal = new() {
							Title = "Other Problems",
							CustomId = "ticket_create|other|reason",
						};
						modal.AddTextInput("Description", "ticket_create|other|description", TextInputStyle.Paragraph, "Please describe, what you're having problems with:", 2, 200, true);
						await this.RespondWithModalAsync(modal.Build());
						break;
				}

				embed.AddBlankField(false);
				embed.AddField("Did this information help you?", "Please close the ticket, if your problem is solved.\nAfter you didn't reply within 30 days, your ticket will automatically be closed.", false);

				buttons.AddRow(await TicketCreateButton.CreateActionRow());

				if (!this.Context.Interaction.HasResponded) {
					await TicketCreateButton.CreateChannelAsync(this.Context.Guild, this.Context.User, $"{this.Context.User.Mention} your ticket is created here:", embed.Build(), buttons.Build());
					await this.RespondAsync("You Ticket is created!", ephemeral: true);
				}
			}
			else {
				await this.RespondAsync("There is already an open Ticket! Please use your ticket to get support.", ephemeral: true);
			}
		}

		public static async Task<DiscordEmbedBuilder> CreateEmbed (DiscordSocketClient client) {
			DiscordEmbedBuilder embed = new() {
				Color = new Color(63, 127, 191),
			};
			embed.WithAuthor(client.CurrentUser);
			embed.WithFooter("Develeon64", (await client.GetUserAsync(298215920709664768)).GetAvatarUrl());
			embed.WithCurrentTimestamp();
			return embed;
		}

		public static async Task<ActionRowBuilder> CreateActionRow () {
			ActionRowBuilder row = new();
			row.WithButton("Close Ticket", "ticket_action|close_ticket");
			row.WithButton("Need more help", "ticket_action|more_help", ButtonStyle.Secondary);
			return row;
		}

		public static async Task<RestTextChannel> CreateChannelAsync (SocketGuild guild, SocketUser user, string message, Embed embed, MessageComponent components) {
			RestTextChannel channel = await guild.CreateTextChannelAsync($"ticket-{user.Username}", (c) => {
				c.CategoryId = Program.globalConfig.getObject("channels").ToObject<JObject>()["ticketCategoryID"].ToObject<ulong>();
				List<Overwrite> overwrites = guild.GetCategoryChannel((ulong)c.CategoryId.Value).PermissionOverwrites.ToList();
				overwrites.Add(new Overwrite(user.Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow)));
				c.PermissionOverwrites = overwrites;
			});

			await channel.SendMessageAsync(message, embed: embed, components: components);
			return channel;
		}
	}
}
