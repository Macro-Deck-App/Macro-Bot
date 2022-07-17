using Develeon64.MacroBot.Utils;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Develeon64.MacroBot.Logging;
using Develeon64.MacroBot.Services;
using Discord.Interactions;

namespace Develeon64.MacroBot {
	public class Program {
		private static InteractionCommandHandler commandHandler;
		private static DiscordSocketClient _client;

		public static Task Main (string[] args) => new Program().MainAsync(args);

		public async Task MainAsync (string[] args) {
			DiscordSocketConfig config = new() {
				AlwaysDownloadUsers = true,
				MaxWaitBetweenGuildAvailablesBeforeReady = 15 * 1000,
				MessageCacheSize = 100,
				GatewayIntents = GatewayIntents.All,
			};
			_client = new DiscordSocketClient(config);
			_client.Log += this.Log;

			_client.Ready += this.Ready;
			_client.MessageReceived += this.MessageReceived;
			_client.UserJoined += this.UserJoined;
			_client.UserLeft += this.UserLeft;

			commandHandler = new InteractionCommandHandler(_client, new InteractionService(_client.Rest));
			await commandHandler.InitializeAsync();

			await _client.LoginAsync(TokenType.Bot, Token.DiscordToken);
			await _client.StartAsync();

			await Task.Delay(-1);
		}

		private async Task Ready () {
			await this.UpdateMemberCount();

			// Load Slash Commands
			// If debug, only load to test guild (Faster)
			// If not debug, load globally
			if (IsDebug())
			{
				await commandHandler.GetInteractionService().RegisterCommandsToGuildAsync(963548230300348416, true);
			}
			else
			{
				await commandHandler.GetInteractionService().RegisterCommandsGloballyAsync(true);
			}
		}

		private async Task UserJoined (SocketGuildUser member) {
			await this.MemberMovement(member, true);
		}

		private async Task UserLeft (SocketGuild guild, SocketUser user) {
			await this.MemberMovement(user as SocketGuildUser, false);
		}

		private async Task MessageReceived (SocketMessage message) {
			SocketGuildUser member = message.Author as SocketGuildUser;
			if (member.IsBot || member.Roles.Contains(member.Guild.GetRole(963570325939945565)))
				return;
			ulong[] imageChannels = { 998289359260368937, 998289378868736070 };

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
		}

		private Task Log (LogMessage logMessage) {
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

			await (member.Guild.GetChannel(963557777584832522) as SocketTextChannel).SendMessageAsync(embed: embed.Build());
		}

		private async Task<int> UpdateMemberCount () {
			SocketGuild guild = _client.GetGuild(963548230300348416);
			int memberCount = 0;
			List<string> memberNames = new();
			foreach (SocketGuildUser member in guild.Users) {
				if (!member.IsBot && !memberNames.Contains(member.Username.ToLower())) {
					memberCount++;
					memberNames.Add(member.Username.ToLower());
				}
			}

			SocketGuildChannel channel = guild.GetChannel(963557777584832522);
			string channelName = String.Empty;
			string[] nameParts = channel.Name.Split("_");
			for (int i = 0; i < nameParts.Length - 1; i++)
				channelName += nameParts[i];
			await channel.ModifyAsync((GuildChannelProperties properties) => { properties.Name = $"{channelName}_{memberCount}"; });
			Logger.Info(Modules.Bot, $"Users on the server: {memberCount}");
			return memberCount;
		}

		static bool IsDebug()
		{
			#if DEBUG
				return true;
			#else
                return false;
			#endif
		}
	}
}
