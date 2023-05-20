using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;
using MacroBot.Extensions;
using MacroBot.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroBot.Discord.Modules.Info;

[Group("info", "Show information about the bot, server, channel, or user")]
[UsedImplicitly]
public class BotInfoModule : InteractionModuleBase<SocketInteractionContext>
{
	private readonly ILogger _logger = Log.ForContext<DiscordService>();


	[SlashCommand("bot", "Show information about the bot")]
	public async Task Bot()
	{
		EmbedBuilder embed = new()
		{
			Title = Context.Client.CurrentUser.Username,
			Description = "Here you can find the information of this Discord Bot."
		};
		embed.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());
		embed.AddField("Description",
			"This is the official Discord Bot for the Macro Deck discord server by <@!300244123569487873>.");
		embed.AddField("Usage",
			"This bot functions using Discord's Slash Command feature, type `/` in chat in order to view a list of valid commands.");
		embed.AddField("Version", $"`{Assembly.GetExecutingAssembly().GetName().Version}`", true);
		embed.AddField("API Latency", $"`{Context.Client.Latency} ms`", true);
		embed.AddField("Written by",
			"<@!298215920709664768>\n<@!367398650076463118>\n<@!908002848967626842>\n<@!300244123569487873>", true);
		embed.WithFooter(Context.Guild.Name + " | " + Context.Guild.Id, Context.Guild.IconUrl);

		await RespondAsync(embed: embed.Build(), ephemeral: true);
	}

	[SlashCommand("message", "Show information about a message")]
	public async Task messageInfo(
		[Summary(description: "Message URL")] string? messageUrl = null,
		[Summary(description: "Message ID (use Developer mode!)")]
		string? messageId = null,
		[Summary(description: "Channel ID (use Developer Mode!)")]
		string? channelId = null)
	{
		IMessage? msg = null;

		try
		{
			if (messageUrl == null && messageId == null)
			{
				var embedBuilder = new EmbedBuilder
				{
					Title = "Argument expected",
					Description = $"At least `messageUrl` or `messageId` should be defined!"
				};
				embedBuilder.WithColor(new Color(255, 50, 50));

				await RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
				return;
			}

			if (messageId is null && channelId is null)
			{
				var ids = messageUrl?.Remove($"https://discord.com/channels/").Split("/")
					.Select(x => Convert.ToUInt64(x))
					.ToArray() ?? Array.Empty<ulong>();
				msg = await Context.Client.GetGuild(ids[0]).GetTextChannel(ids[1]).GetMessageAsync(ids[2]);
			}
			else if (messageUrl is null && channelId is null)
			{
				msg = await Context.Channel.GetMessageAsync(Convert.ToUInt64(messageId));
			}
			else if (messageUrl is null)
			{
				msg = await Context.Guild.GetTextChannel(Convert.ToUInt64(channelId))
					.GetMessageAsync(Convert.ToUInt64(messageId));
			}

			EmbedBuilder embed = new()
			{
				Title = $"Message {msg?.Id}",
			};
			AddFieldToEmbedIfNotNull(embed, "Content", msg?.Content);
			AddFieldToEmbedIfNotNull(embed, "Author", msg?.Author?.Mention, true);
			AddFieldToEmbedIfNotNull(embed, "Channel", $"<#{msg?.Channel?.Id}>", true);
			AddFieldToEmbedIfNotNull(
				embed, "Attachments / Embeds", $"{msg?.Attachments?.Count} / {msg?.Embeds?.Count}", true);
			AddFieldToEmbedIfNotNull(embed, "Content", msg?.Content);

			var avatarUrl = msg?.Author?.GetAvatarUrl();
			if (avatarUrl != null)
			{
				embed.WithThumbnailUrl(avatarUrl);
			}
			
			await RespondAsync(embed: embed.Build(),
				components: new ComponentBuilder()
					.WithButton("Jump to message", style: ButtonStyle.Link, url: msg.GetJumpUrl())
					.Build(), ephemeral: true);
		}
		catch (FormatException)
		{
			var embedBuilder = new EmbedBuilder
			{
				Title = "Argument not expected",
				Description = $"`messageUrl` or `channelId` must be a number!"
			};
			embedBuilder.WithColor(new Color(255, 50, 50));

			await RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
		}
		catch (Exception e)
		{
			_logger.Error(e, "Cannot get message!");
			await RespondAsync("An error occured while getting the message. Please try again later.", ephemeral: true);
		}
	}

	[UserCommand("Show user information")]
	public async Task userInfoUC(SocketGuildUser user)
	{
		await userInfo(user);
	}

	[UserCommand("Show user information")]
	[SlashCommand("user", "Show information about a user")]
	public async Task userInfo(
		[Summary(description: "Shows information about the user")]
		SocketGuildUser? user = null,
		bool asUser = false)
	{
		user ??= Context.Guild.GetUser(Context.User.Id);

		if (user == null)
		{
			return;
		}
		
		EmbedBuilder embed = new();
		if (user.IsBot != true || asUser)
		{
			embed.Title = user.Username;
			embed.Description = "Here you can find the information of " + $"<@!{user.Id}>" + ".";
			embed.WithThumbnailUrl(Context.Client.GetUser(user.Id).GetAvatarUrl());
			embed.AddField("Status", Context.Client.GetUser(user.Id).Status, true);
			var acts = string.Empty;
			foreach (var activities in user.Activities)
			{
				switch (activities)
				{
					case SpotifyGame spot:
						if (spot.Elapsed.HasValue && spot.Duration.HasValue)
						{
							acts +=
								$"[Spotify] [**{spot.TrackTitle}**]({spot.TrackUrl}) {string.Join(", ", spot.Artists)} ({spot.Elapsed.Value:hh\\:mm\\:ss} / {spot.Duration.Value:hh\\:mm\\:ss})\r\n";
						}

						break;
					case CustomStatusGame customStatus:
						acts +=
							$"[{customStatus.Name}] **{customStatus.Emote.Name} {customStatus.Details} {customStatus.State}**\r\n";
						break;
					case RichGame richGame:
						acts +=
							$"[{richGame.Name}]{(!string.IsNullOrEmpty(richGame.Details) ? $" **{richGame.Details}**" : " No details")}\r\n";
						break;
					default:
						acts +=
							$"[{activities.Type}] **{activities.Name}** {(!string.IsNullOrEmpty(activities.Details) ? $" {activities.Details}" : " No details")}\r\n";
						break;
				}
			}

			embed.AddField("Activities", acts == string.Empty ? "None" : acts);
			embed.AddField("Roles", string.Join(", ", Context.Guild.GetUser(user.Id).Roles.Select(r => r.Mention)),
				true);
			embed.AddField("Creation Date", $"<t:{Context.Client.GetUser(user.Id).CreatedAt.ToUnixTimeSeconds()}>");
			var userJoinedDateTimeOffset = Context.Guild.GetUser(user.Id).JoinedAt;
			if (userJoinedDateTimeOffset != null)
			{
				embed.AddField("Join Date", $"<t:{userJoinedDateTimeOffset.Value.ToUnixTimeSeconds()}>");
			}

			embed.AddField("Active Clients", string.Join(", ", user.ActiveClients));

			embed.WithFooter("ID: " + Context.Client.GetUser(user.Id).Id);
		}
		else
		{
			embed.Title = user.Username;
			embed.Description = "Here you can find the information of the bot " + $"<@!{user.Id}>" + ".";
			embed.WithThumbnailUrl(Context.Client.GetUser(user.Id).GetAvatarUrl());
			embed.AddField("Status", Context.Client.GetUser(user.Id).Status, true);
			embed.AddField("Creation Date", $"<t:{Context.Client.GetUser(user.Id).CreatedAt.ToUnixTimeSeconds()}>");
			var userJoinedAtDateTimeOffset = Context.Guild.GetUser(user.Id).JoinedAt;
			if (userJoinedAtDateTimeOffset != null)
			{
				embed.AddField("Join Date", $"<t:{userJoinedAtDateTimeOffset.Value.ToUnixTimeSeconds()}>");
			}

			embed.WithFooter("ID: " + Context.Client.GetUser(user.Id).Id);
		}

		if (user.Status == UserStatus.Online)
		{
			embed.WithColor(Color.Green);
		}
		else if (user.Status == UserStatus.Offline || user.Status == UserStatus.Invisible)
		{
			embed.WithColor(Color.LightGrey);
		}
		else if (user.Status == UserStatus.Idle)
		{
			embed.WithColor(Color.Gold);
		}
		else if (user.Status == UserStatus.DoNotDisturb)
		{
			embed.WithColor(Color.Red);
		}
		else
		{
			embed.WithColor(Color.DarkOrange);
		}

		if (user.IsBot)
		{
			embed.WithColor(new Color(88, 101, 242));
		}

		await RespondAsync(embed: embed.Build(), ephemeral: true);
	}

	[SlashCommand("guild", "Show information about the guild (same as /info server)")]
	public async Task GuildInfo()
	{
		await DeferAsync(true);

		try
		{
			EmbedBuilder embed = new()
			{
				Title = Context.Guild.Name,
				Description = "Here you can find the information of this server."
			};
			embed.WithThumbnailUrl(Context.Guild.IconUrl);
			embed.AddField("Owner", "<@" + Context.Guild.OwnerId + ">", true);
			embed.AddField("Members", Context.Guild.MemberCount, true);
			embed.AddField("Text Channels", Context.Guild.TextChannels.Count, true);
			embed.AddField("Voice Channels", Context.Guild.VoiceChannels.Count, true);
			embed.AddField("Roles", Context.Guild.Roles.Count, true);
			embed.AddField("Default Channel", Context.Guild.DefaultChannel.Mention, true);
			embed.AddField("Description", Context.Guild.Description ?? "No description");
			var enumValues = Enum.GetValues(typeof(GuildFeature))
				.Cast<GuildFeature>()
				.Where(f => Context.Guild.Features.Value.HasFlag(f))
				.ToArray();
			embed.AddField("Features", string.Join(",\r\n", enumValues.Select(e => e.ToString())));
			embed.WithFooter($"ID: {Context.Guild.Id}");

			await FollowupAsync(embed: embed.Build(), ephemeral: true);
		}
		catch (Exception e)
		{
			_logger.Error(e, "Can't create embed!");
		}
	}

	[SlashCommand("server", "Show information about the server (same as /info guild)")]
	public async Task serverInfo()
	{
		await GuildInfo();
	}

	[SlashCommand("channel", "Show information about a specific text channel")]
	public async Task channelInfo(
		[Summary(description: "Show information of specified channel")]
		ITextChannel? channel = null)
	{
		if (channel == null)
		{
			EmbedBuilder embed = new()
			{
				Title = "#" + Context.Channel.Name,
				Description = $"Here you can find the information of <#{Context.Channel.Id}>."
			};
			embed.AddField("Created", $"<t:{Context.Channel.CreatedAt.ToUnixTimeSeconds()}>", true);
			embed.AddField("Type", Context.Channel.GetChannelType(), true);
			embed.WithFooter($"ID: {Context.Channel.Id}");

			await RespondAsync(embed: embed.Build(), ephemeral: true);
		}
		else
		{
			EmbedBuilder embed = new()
			{
				Title = "#" + channel.Name,
				Description = $"Here you can find the information of <#{channel.Id}>."
			};
			embed.AddField("Created", $"<t:{channel.CreatedAt.ToUnixTimeSeconds()}>", true);
			embed.WithFooter($"ID: {channel.Id}");

			await RespondAsync(embed: embed.Build(), ephemeral: true);
		}
	}
	
	private static void AddFieldToEmbedIfNotNull(EmbedBuilder embedBuilder,
												string name,
												string? value,
												bool inline = false)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return;
		}
		
		embedBuilder.AddField(name, value, inline);
	}
}