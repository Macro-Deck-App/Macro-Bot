﻿using Develeon64.MacroBot.Utils;
using Discord;
using Newtonsoft.Json;
using Discord.Net;
using Discord.WebSocket;
using Develeon64.MacroBot.Logging;
using Develeon64.MacroBot.Services;
using Develeon64.MacroBot.Commands;
using Discord.Interactions;
using Develeon64.MacroBot.Models;
using System.Timers;

namespace Develeon64.MacroBot {
	public class Program {
		private static InteractionCommandHandler commandHandler;
		private static DiscordSocketClient _client;
		public ulong messageid = 1;
		public List<List<string>> stringss = new();

		public static Task Main (string[] args) => new Program().MainAsync(args);

		public async Task MainAsync (string[] args) {
			System.Timers.Timer timer = new() {
				AutoReset = true,
				Enabled = true,
				Interval = (int)new TimeSpan(1, 0, 0, 0, 0).TotalMilliseconds,
			};
			timer.Elapsed += this.Timer_Elapsed;

			Directory.CreateDirectory("DB");
			await DatabaseManager.Initialize("DB/Database.db3");

			DiscordSocketConfig config = new() {
				AlwaysDownloadUsers = true,
				MaxWaitBetweenGuildAvailablesBeforeReady = (int)new TimeSpan(0, 0, 15).TotalMilliseconds,
				MessageCacheSize = 100,
				GatewayIntents = GatewayIntents.All,
			};
			_client = new DiscordSocketClient(config);
			_client.Log += this.Log;

			_client.Ready += this.Ready;
			_client.MessageReceived += this.MessageReceived;
			_client.UserJoined += this.UserJoined;
			_client.UserLeft += this.UserLeft;
			_client.ChannelDestroyed += this.ChannelDestroyed;
			_client.ThreadCreated += async (thread) => await ThreadCreated(thread);
			string str = "-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-";
			stringss.Add(str.Split(",").ToList());
			stringss.Add(str.Split(",").ToList());
			stringss.Add(str.Split(",").ToList());
			stringss.Add(str.Split(",").ToList());
			stringss.Add(str.Split(",").ToList());
			
			System.Timers.Timer timer2 = new System.Timers.Timer(25 * 1000);
			timer2.Elapsed += async (obj, args) => stringss = await StatusLoop.StatusLoop1(_client, messageid, stringss); // Which can also be written as += new ElapsedEventHandler(OnTick);
			timer2.Start();

			commandHandler = new InteractionCommandHandler(_client, new InteractionService(_client.Rest));
			await commandHandler.InitializeAsync();

			await _client.LoginAsync(TokenType.Bot, ConfigManager.GlobalConfig.Token);
			await _client.StartAsync();

			await Task.Delay(-1);
		}

		public async Task ButtonExecuted(SocketMessageComponent component, SocketUser user) {
            //if (component.User != user) { return; }

            if (component.Data.CustomId == "plugin-problem-yes") {
                await component.Message.DeleteAsync();
                await component.Channel.SendMessageAsync($"<@{prevplauserid}>, {user.Mention} has a problem on your plugin.");
                await (component.Channel as SocketThreadChannel)!.ModifyAsync(msg => msg.Name = @$"{component.Channel.Name} (Plugin Problem - {prevplugin})");
                await (component.Channel as SocketThreadChannel)!.LeaveAsync();
            } else if (component.Data.CustomId == "plugin-problem-no") {
                await component.Message.DeleteAsync();
                var msg = await component.Channel.SendMessageAsync("Got it. Thank you for the clarification.");
                await Task.Delay(5000);
                await msg.DeleteAsync();
                await (component.Channel as SocketThreadChannel)!.LeaveAsync();
            } else { return; }
        }

		public string prevthread = "";
        public ulong prevplauserid = 0;
        public string prevplugin = "";

        public async Task ThreadCreated(SocketThreadChannel thread) {
            var msg = await thread.GetMessagesAsync(2).FlattenAsync();
            var lastmsg = msg.Last();
            var extension = JsonConvert.DeserializeObject<List<Extension>>(await HTTPRequest.GetAsync($"https://extensionstore.api.macro-deck.app/Extensions"));

            foreach (var ext in extension) {
                if (prevthread == thread.Name) { return; }

                if ((lastmsg.CleanContent.IndexOf(ext.name, StringComparison.OrdinalIgnoreCase) >= 0) || (lastmsg.CleanContent.IndexOf(ext.packageId, StringComparison.OrdinalIgnoreCase) >= 0) || (thread.Name.IndexOf(ext.name, StringComparison.OrdinalIgnoreCase) >= 0) || (thread.Name.IndexOf(ext.packageId, StringComparison.OrdinalIgnoreCase) >= 0)) {
                    EmbedBuilder embed = new EmbedBuilder() {
                        Title = $"Is your problem is this plugin?",
                        Description = $"Macro Bot detects a plugin name on your post.\r\nIf your problem is this plugin, click Yes. Otherwise, click No."
                    };

                    embed.AddField("Name", $"{ext.name} ({ext.packageId})", true);
                    embed.AddField("Author", (ext.dSupportUserId is not null)? $"<@{ext.dSupportUserId}>" : ext.author, true);
                    prevthread = thread.Name;
                    prevplauserid = (ulong)ext.dSupportUserId!;
                    prevplugin = ext.packageId!;

                    ComponentBuilder componentb = new ComponentBuilder()
                        .WithButton("Yes", "plugin-problem-yes", ButtonStyle.Success)
                        .WithButton("No", "plugin-problem-no", ButtonStyle.Danger);

                    _client.ButtonExecuted -= async (component) => await ButtonExecuted(component, thread.Owner);
                    _client.ButtonExecuted += async (component) => await ButtonExecuted(component, thread.Owner);

                    await thread.SendMessageAsync(embed: embed.Build(), components: componentb.Build());
                }
            }
        }

		private async void Timer_Elapsed (object? sender, ElapsedEventArgs e) {
			DateTime now = DateTime.Now.ToUniversalTime();
			if (now.Hour == 5) {
				foreach (Ticket ticket in await DatabaseManager.GetTickets()) {
					try {
						int days = (int)now.Subtract(ticket.Modified).TotalDays;
						if (days > 30) {
							await DatabaseManager.DeleteTicket(ticket.Channel, IdType.Channel);
							var user = await _client.GetUserAsync(ticket.Author);
							await (await _client.GetChannelAsync(ticket.Channel) as SocketTextChannel).DeleteAsync();
							await user.SendMessageAsync($"Your ticket on the Macro-Deck Support-Server was automatically closed, because of 30 days of inactivity.\nIf you still need help, please open a new ticket.");
						}
						else if (days > 28) {
							SocketUser user = await _client.GetUserAsync(ticket.Author) as SocketUser;
							//await user.SendMessageAsync($"Hey there,\njust as a reminder: you have an open ticket on the Macro-Deck Support-Server, but haven't replied for within some days.\nIf you still need help, please reactivate the ticket by replying in <#{ticket.Channel}>, otherwise your ticket will be closed in ***{(30 - days) + 1} days***.");
							await ((await _client.GetChannelAsync(ticket.Channel)) as SocketTextChannel).SendMessageAsync($"Hey {user?.Mention},\nyour open ticket will be closed in ***{(30 - days) + 1} days*** due to inactivity.\nIs your problem fixed, or do you still need help?");
						}
					}
					catch (Exception ex) { }
				}
			}
		}

		private async Task Ready () {
			await this.UpdateMemberCount();

			// Load Slash Commands
			// If debug, only load to test guild (Faster)
			// If not debug, load globally
			//if (IsDebug())
			//{
				//await commandHandler.GetInteractionService().RegisterCommandsToGuildAsync(ConfigManager.GlobalConfig.TestGuildId, true);
			//}
			//else
			//{
				await commandHandler.GetInteractionService().RegisterCommandsGloballyAsync(true);
			//}

			stringss = await StatusLoop.StatusLoop1(_client, 1, stringss);
		}

		private async Task UserJoined (SocketGuildUser member) {
			await this.MemberMovement(member, true);
		}

		private async Task UserLeft (SocketGuild guild, SocketUser user) {
			await this.MemberMovement(user as SocketGuildUser, false);

			await (await _client.GetChannelAsync((await DatabaseManager.GetTicket(user.Id)).Channel) as SocketTextChannel).DeleteAsync();
			await DatabaseManager.DeleteTicket(user.Id, IdType.User);
		}

		private async Task MessageReceived (SocketMessage message) {
			if (message.Author is not SocketGuildUser member || member.IsBot || member.Roles.Contains(member.Guild.GetRole(ConfigManager.GlobalConfig.Roles.ModeratorRoleId)))
				return;
			ulong[] imageChannels = ConfigManager.GlobalConfig.Channels.ImageOnlyChannels;

			if (message.MentionedEveryone) {
				await message.DeleteAsync();
				Logger.Info(Modules.Bot, $"Message containing @everyone from {message.Author.Username}#{message.Author.Discriminator} in {message.Channel.Name} was deleted!");
			}
			else if (imageChannels.Contains(message.Channel.Id)) {
				await message.DeleteAsync();

				try {
					DiscordEmbedBuilder embed = new DiscordEmbedBuilder() {
						Color = new Color(63, 127, 191),
						Description = message.CleanContent.Replace("<", "\\<").Replace("*", "\\*").Replace("_", "\\_").Replace("`", "\\`").Replace(":", "\\:"),
						Timestamp = message.Timestamp,
						Title = "__Yout Post__",
					};
					embed.WithAuthor(member.Guild.Name, member.Guild.IconUrl);

					await member.SendMessageAsync(text: $"The channel ${message.Channel} is only meant for images.\nHere is your text, so that you don't need to rewrite it into another channel:", embed: embed.Build());
					Logger.Info(Modules.Bot, $"Message without image from {message.Author.Username}#{message.Author.Discriminator} in {message.Channel.Name} was deleted! DM with their text was successfully sent.");

				}
				catch (HttpException) {
					Logger.Info(Modules.Bot, $"Message without image from {message.Author.Username}#{message.Author.Discriminator} in {message.Channel.Name} was deleted! DM with their text was not sent.");
				}

			}
			if (message.Channel.Name.StartsWith("ticket-"))
				await DatabaseManager.UpdateTicket(member.Id);
		}

		private async Task ChannelDestroyed (SocketChannel channel) {
			if (channel is SocketTextChannel textChannel && textChannel.Name.StartsWith("ticket-")) {
				await DatabaseManager.DeleteTicket(channel.Id, IdType.Channel);
			}
		}

		private Task Log (LogMessage logMessage) {
			if (logMessage.Message == null) return Task.CompletedTask;

			Modules module = logMessage.Source switch {
				"Gateway" => Modules.Gateway,
				"Discord" => Modules.Discord,
				_ => Modules.Library,
			};
			Levels level = logMessage.Severity switch {
				LogSeverity.Critical => Levels.Critical,
				LogSeverity.Error => Levels.Error,
				LogSeverity.Warning => Levels.Warning,
				LogSeverity.Info => Levels.Info,
				LogSeverity.Verbose => Levels.Verbose,
				LogSeverity.Debug => Levels.Debug,
				_ => Levels.Trace,
			};
			Logger.Log(module, logMessage.Message.StartsWith("Discord.Net") ? Levels.Trace : level, logMessage.Message);
			return Task.CompletedTask;
		}

		private async Task MemberMovement (SocketGuildUser member, bool joined) {
			DiscordEmbedBuilder embed = new() {
				Color = joined ? new Color(191, 63, 127) : new Color(191, 127, 63),
				Description = $"Latest member count: **{await this.UpdateMemberCount()}**",
				ThumbnailUrl = member.GetAvatarUrl(),
				Title = $"__**{member.Username}#{member.Discriminator} {(joined ? "joined" : "left")} the server!**__",
				Footer = new EmbedFooterBuilder().WithText("Develeon64").WithIconUrl(member.Guild.GetUser(298215920709664768).GetAvatarUrl()),
			};
			embed.WithAuthor(_client.CurrentUser);
			embed.WithCurrentTimestamp();

			embed.AddField("__ID__", member.Id, member.Nickname != null);
			if (member.Nickname != null) {
				embed.AddField("__Nickname__", member.Nickname, true);
				embed.AddBlankField(true);
			}
			embed.AddField("__Joined__", $"<t:{member.JoinedAt?.ToUnixTimeSeconds()}:R>", true);
			embed.AddField("__Created__", $"<t:{member.CreatedAt.ToUnixTimeSeconds()}:R>", true);

			await (member.Guild.GetChannel(ConfigManager.GlobalConfig.Channels.MemberScreeningChannelId) as SocketTextChannel).SendMessageAsync(embed: embed.Build());
		}

		private async Task<int> UpdateMemberCount () {
			SocketGuild guild = _client.GetGuild(ConfigManager.GlobalConfig.TestGuildId);
			int memberCount = 0;
			List<string> memberNames = new();
			foreach (SocketGuildUser member in guild.Users) {
				if (!member.IsBot && !memberNames.Contains(member.Username.ToLower())) {
					memberCount++;
					memberNames.Add(member.Username.ToLower());
				}
			}

			SocketGuildChannel channel = guild.GetChannel(ConfigManager.GlobalConfig.Channels.MemberScreeningChannelId);
			string channelName = String.Empty;
			string[] nameParts = channel.Name.Split("_");
			for (int i = 0; i < nameParts.Length - 1; i++)
				channelName += nameParts[i];
			await channel.ModifyAsync((GuildChannelProperties properties) => { properties.Name = $"{channelName}_{memberCount}"; });
			Logger.Info(Modules.Bot, $"Users on the server: {memberCount}");
			return memberCount;
		}

		static bool IsDebug () {
			#if DEBUG
				return true;
			#else
				return false;
			#endif
		}
	}
}
