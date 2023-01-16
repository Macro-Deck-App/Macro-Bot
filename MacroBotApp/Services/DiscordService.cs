using System.Net.Http.Json;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using MacroBot.Config;
using MacroBot.Discord;
using MacroBot.Extensions;
using MacroBot.Models;
using MacroBot.ServiceInterfaces;
using MacroBot.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MacroBot.Services;

public class DiscordService : IDiscordService, IHostedService
{
	private readonly ILogger _logger = Log.ForContext<DiscordService>();
	
	private readonly BotConfig _botConfig;
	private readonly DiscordSocketClient _discordSocketClient;
	private readonly IServiceProvider _serviceProvider;
	private readonly IStatusCheckService _statusCheckService;
	private readonly InteractionService _interactionService;
	private readonly IHttpClientFactory _httpClientFactory;

	private ulong _updateMessageId = 1;

	private string prevthread = "";
    private ulong prevplauserid = 0;
    private string prevplugin = "";

    public DiscordService(BotConfig botConfig,
	    DiscordSocketClient discordSocketClient,
	    IServiceProvider serviceProvider,
	    IStatusCheckService statusCheckService,
	    InteractionService interactionService,
	    IHttpClientFactory httpClientFactory)
    {
	    _botConfig = botConfig;
	    _discordSocketClient = discordSocketClient;
	    _serviceProvider = serviceProvider;
	    _statusCheckService = statusCheckService;
	    _interactionService = interactionService;
	    _httpClientFactory = httpClientFactory;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
	    return Task.CompletedTask;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
	    await InitializeDiscord();
	    InitializeStatusCheckService();
    }

    private void InitializeStatusCheckService()
    {
	    _statusCheckService.ItemStatusInCollectionChanged += StatusCheckServiceOnItemStatusInCollectionChanged;
    }

    private async void StatusCheckServiceOnItemStatusInCollectionChanged(object? sender, EventArgs e)
    {
	    await UpdateStatusMessage();
    }

    private async Task InitializeDiscord()
    {
	    _discordSocketClient.UseSerilog();
	    _discordSocketClient.Ready += Ready;
	    _discordSocketClient.MessageReceived += MessageReceived;
	    _discordSocketClient.UserJoined += UserJoined;
	    _discordSocketClient.UserLeft += UserLeft;
	    _discordSocketClient.ThreadCreated += async (thread) => await ThreadCreated(thread);

	    await _serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync();
	    await _discordSocketClient.LoginAsync(TokenType.Bot, _botConfig.Token);
	    await _discordSocketClient.StartAsync();
    }

    private async Task ButtonExecuted(SocketMessageComponent component, IMentionable user)
    {
	    switch (component.Data.CustomId)
	    {
		    case "plugin-problem-yes":
			    await component.Message.DeleteAsync();
			    await component.Channel.SendMessageAsync($"<@{prevplauserid}>, {user.Mention} has a problem on your plugin.");
			    await (component.Channel as SocketThreadChannel)!.ModifyAsync(msg => msg.Name = @$"{component.Channel.Name} (Plugin Problem - {prevplugin})");
			    await (component.Channel as SocketThreadChannel)!.LeaveAsync();
			    break;
		    case "plugin-problem-no":
		    {
			    await component.Message.DeleteAsync();
			    var msg = await component.Channel.SendMessageAsync("Got it. Thank you for the clarification.");
			    await Task.Delay(5000);
			    await msg.DeleteAsync();
			    await (component.Channel as SocketThreadChannel)!.LeaveAsync();
			    break;
		    }
		    default:
			    return;
	    }
    }

    private async Task ThreadCreated(SocketThreadChannel thread) {
		var msg = await thread.GetMessagesAsync(2).FlattenAsync();
		var lastMsg = msg.Last();
		using var httpClient = _httpClientFactory.CreateClient();
		var extension = await httpClient.GetFromJsonAsync<List<Extension>>("https://extensionstore.api.macro-deck.app/Extensions");
		
		foreach (var ext in extension) {
			if (prevthread == thread.Name) { return; }

			if (lastMsg.CleanContent.IndexOf(ext.Name, StringComparison.OrdinalIgnoreCase) < 0 &&
			    lastMsg.CleanContent.IndexOf(ext.PackageId, StringComparison.OrdinalIgnoreCase) < 0 &&
			    thread.Name.IndexOf(ext.Name, StringComparison.OrdinalIgnoreCase) < 0 &&
			    thread.Name.IndexOf(ext.PackageId, StringComparison.OrdinalIgnoreCase) < 0) continue;
			
			var embed = new EmbedBuilder() {
				Title = $"Is your problem is this plugin?",
				Description = $"Macro Bot detects a plugin name on your post.\r\nIf your problem is this plugin, click Yes. Otherwise, click No."
			};

			embed.AddField("Name", $"{ext.Name} ({ext.PackageId})", true);
			embed.AddField("Author", ext.DSupportUserId is not null? $"<@{ext.DSupportUserId}>" : ext.Author, true);
			prevthread = thread.Name;
			prevplauserid = (ulong)ext.DSupportUserId!;
			prevplugin = ext.PackageId!;

			var components = new ComponentBuilder()
				.WithButton("Yes", "plugin-problem-yes", ButtonStyle.Success)
				.WithButton("No", "plugin-problem-no", ButtonStyle.Danger);

			_discordSocketClient.ButtonExecuted -= async (component) => await ButtonExecuted(component, thread.Owner);
			_discordSocketClient.ButtonExecuted += async (component) => await ButtonExecuted(component, thread.Owner);

			await thread.SendMessageAsync(embed: embed.Build(), components: components.Build());
		}
	}

    private async Task Ready () {
	    _logger.Information("Discord ready");
	    await _interactionService.RegisterCommandsGloballyAsync();
		await UpdateMemberCount();
    }

	private async Task UserJoined (SocketGuildUser member) {
		_logger.Information("{User} joined the server", member.Nickname);
		await MemberMovement(member, true);
	}

	private async Task UserLeft (SocketGuild guild, SocketUser user)
	{
		_logger.Information("{User} left the server", user.Username);
		if (user is SocketGuildUser guildUser)
		{
			await MemberMovement(guildUser, false);
		}
	}

	private async Task MessageReceived (SocketMessage message) {
		_logger.Debug("Message received: {Message}", message.Content);
		if (message.Author is not SocketGuildUser member || member.IsBot || member.Roles.Contains(member.Guild.GetRole(_botConfig.Roles.ModeratorRoleId)))
			return;
		var imageChannels = _botConfig.Channels.ImageOnlyChannels;

		if (message.MentionedEveryone) {
			await message.DeleteAsync();
			_logger.Information(
				"Message containing @everyone from {AuthorUsername}#{AuthorDiscriminator} in {ChannelName} was deleted",
				message.Author.Username,
				message.Author.Discriminator,
				message.Channel.Name);
		}
		else if (imageChannels.Contains(message.Channel.Id)) {
			await message.DeleteAsync();

			try {
				var embed = new DiscordEmbedBuilder() {
					Color = new Color(63, 127, 191),
					Description = message.CleanContent.Replace("<", "\\<").Replace("*", "\\*").Replace("_", "\\_").Replace("`", "\\`").Replace(":", "\\:"),
					Timestamp = message.Timestamp,
					Title = "__Yout Post__",
				};
				embed.WithAuthor(member.Guild.Name, member.Guild.IconUrl);

				await member.SendMessageAsync(text: $"The channel ${message.Channel} is only meant for images.\nHere is your text, so that you don't need to rewrite it into another channel:", embed: embed.Build());
				_logger.Information($"Message without image from {message.Author.Username}#{message.Author.Discriminator} in {message.Channel.Name} was deleted! DM with their text was successfully sent.");

			}
			catch (HttpException) {
				_logger.Information($"Message without image from {message.Author.Username}#{message.Author.Discriminator} in {message.Channel.Name} was deleted! DM with their text was not sent.");
			}

		}
	}

	private async Task MemberMovement (SocketGuildUser member, bool joined) {
		DiscordEmbedBuilder embed = new() {
			Color = joined ? new Color(191, 63, 127) : new Color(191, 127, 63),
			Description = $"Latest member count: **{await UpdateMemberCount()}**",
			ThumbnailUrl = member.GetAvatarUrl(),
			Title = $"__**{member.Username}#{member.Discriminator} {(joined ? "joined" : "left")} the server!**__",
			Footer = new EmbedFooterBuilder().WithText("Develeon64").WithIconUrl(member.Guild.GetUser(298215920709664768).GetAvatarUrl()),
		};
		embed.WithAuthor(_discordSocketClient.CurrentUser);
		embed.WithCurrentTimestamp();

		embed.AddField("__ID__", member.Id, member.Nickname != null);
		if (member.Nickname != null) {
			embed.AddField("__Nickname__", member.Nickname, true);
			embed.AddBlankField(true);
		}
		embed.AddField("__Joined__", $"<t:{member.JoinedAt?.ToUnixTimeSeconds()}:R>", true);
		embed.AddField("__Created__", $"<t:{member.CreatedAt.ToUnixTimeSeconds()}:R>", true);

		await (member.Guild.GetChannel(_botConfig.Channels.MemberScreeningChannelId) as SocketTextChannel).SendMessageAsync(embed: embed.Build());
	}
	
	private async Task<int> UpdateMemberCount () {
		var guild = _discordSocketClient.GetGuild(_botConfig.TestGuildId);
		var memberCount = 0;
		List<string> memberNames = new();
		foreach (var member in guild.Users)
		{
			if (member.IsBot || memberNames.Contains(member.Username.ToLower())) continue;
			memberCount++;
			memberNames.Add(member.Username.ToLower());
		}

		var channel = guild.GetChannel(_botConfig.Channels.MemberScreeningChannelId);
		var channelName = string.Empty;
		var nameParts = channel.Name.Split("_");
		for (var i = 0; i < nameParts.Length - 1; i++)
			channelName += nameParts[i];
		await channel.ModifyAsync(properties => { properties.Name = $"{channelName}_{memberCount}"; });
		_logger.Information("Users on the server: {MemberCount}", memberCount);
		return memberCount;
	}

	private async Task UpdateStatusMessage()
	{
		var status = _statusCheckService.LastStatusCheckResults.ToArray();
		var messageEmbed = DiscordStatusCheckMessageBuilder.Build(status);
		var channel = (_discordSocketClient.GetGuild(_botConfig.TestGuildId)
			.GetChannel(_botConfig.Channels.UpdateChannelId) as ITextChannel);
		try {
			var msg = await channel!.GetMessageAsync(_updateMessageId);
			await channel!.ModifyMessageAsync(_updateMessageId, m =>
			{
				m.Content = Constants.StatusUpdateMessageTitle;
			});
		} catch (Exception) {
			await channel!.DeleteMessagesAsync(await channel.GetMessagesAsync(10).FlattenAsync());
			Thread.Sleep(1000);

			var msg = await channel!.SendMessageAsync(Constants.StatusUpdateMessageTitle);
			_updateMessageId = msg.Id;
		}
		await channel!.ModifyMessageAsync(_updateMessageId, m =>
		{
			m.Content = Constants.StatusUpdateMessageTitle;
			m.Embed = messageEmbed;
		});
	}
	
}