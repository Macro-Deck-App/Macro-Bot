using Develeon64.MacroBot.Models;
using Develeon64.MacroBot.Utils;
using Discord;
using Discord.Interactions;
using Discord.Rest;
using Newtonsoft.Json.Linq;
using SQLite;

namespace Develeon64.MacroBot.Commands {
	public class TicketCreateButton : InteractionModuleBase<SocketInteractionContext> {
		[ComponentInteraction("ticket_create|no_connect")]
		public async Task CreateNoConnectTicket () {
			SQLiteAsyncConnection db = new("DB/Database.sqlite");
			try {
				await db.CreateTableAsync<Tickets>();

				Tickets ticket = new() {
					Author = this.Context.User.Id.ToString(),
					//Channel = channel.Id.ToString(),
					//Message = message.Id.ToString(),
					Created = DateTime.Now,
					Modified = DateTime.Now,
				};
				await db.InsertAsync(ticket);

				RestTextChannel channel = await this.Context.Guild.CreateTextChannelAsync($"ticket-{this.Context.User.Username}", (c) => { c.CategoryId = Program.globalConfig.getObject("channels").ToObject<JObject>()["ticketCategoryID"].ToObject<ulong>(); });
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
				ticket.Channel = channel.Id.ToString();
				ticket.Message = message.Id.ToString();
				await db.UpdateAsync(ticket);
				await this.RespondAsync("Your ticket was created.", ephemeral: true);
			}
			catch (SQLiteException ex) {
				Console.WriteLine(ex.Message);
				if (ex.Message.ToLower() == "constraint") {
					await this.Context.Channel.SendMessageAsync("NO!");
				}
			}
			finally {
				//db.GetConnection().Close();
			}

		}
	}
}
