using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using JetBrains.Annotations;
using MacroBot.Config;
using MacroBot.Discord;
using MacroBot.Extensions;
using MacroBot.Models.Extensions;
using MacroBot.Models.Status;
using MacroBot.Models.Webhook;
using MacroBot.ServiceInterfaces;
using Microsoft.VisualBasic;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroBot.Services;

[UsedImplicitly]
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
	private int _currentUpdateEmbedHash = 0;
	
	public bool DiscordReady { get; private set; }

	private string prevthread = "";
    private ulong? prevplauserid = 0;
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
	    _statusCheckService.StatusCheckFinished += StatusCheckServiceOnStatusCheckFinished;
    }

    private async void StatusCheckServiceOnStatusCheckFinished(object? sender, StatusCheckFinishedEventArgs e)
    {
	    try
	    {
		    await UpdateStatusMessage();
	    }
	    catch (Exception ex)
	    {
		    _logger.Fatal(ex, "Error while updating status check message");
	    }
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

    private async Task ThreadCreated(SocketThreadChannel thread) {
		var msg = await thread.GetMessagesAsync(2).FlattenAsync();
		var lastMsg = msg.Last();
		using var httpClient = _httpClientFactory.CreateClient();
		var extensions = await httpClient.GetFromJsonAsync<List<Extension>>("https://extensionstore.api.macro-deck.app/Extensions");

		if (extensions is null)
		{
			return;
		}
		
		foreach (var extension in extensions) {
			if (prevthread == thread.Name)
			{
				return;
			}

			if (lastMsg.CleanContent.IndexOf(extension.Name, StringComparison.OrdinalIgnoreCase) < 0 &&
			    lastMsg.CleanContent.IndexOf(extension.PackageId, StringComparison.OrdinalIgnoreCase) < 0 &&
			    thread.Name.IndexOf(extension.Name, StringComparison.OrdinalIgnoreCase) < 0 &&
			    thread.Name.IndexOf(extension.PackageId, StringComparison.OrdinalIgnoreCase) < 0) continue;
			
			var embed = new EmbedBuilder {
				Title = $"Do you have a problem with a plugin?",
				Description = $"Macro Bot detects a plugin name on your post.\r\nIf your problem is this plugin, click Yes. Otherwise, click No."
			};

			embed.AddField("Name", $"{extension.Name} ({extension.PackageId})", true);
			embed.AddField("Author", extension.DSupportUserId is not null? $"<@{extension.DSupportUserId}>" : extension.Author, true);
			prevthread = thread.Name;
			prevplauserid = extension.DSupportUserId is not null ? ulong.Parse(extension.DSupportUserId) : null;
			prevplugin = extension.PackageId!;

			var components = new ComponentBuilder()
				.WithButton("Yes", "plugin-problem-yes", ButtonStyle.Success)
				.WithButton("No", "plugin-problem-no", ButtonStyle.Danger);

			_discordSocketClient.ButtonExecuted -= DiscordSocketClientOnButtonExecuted;
			_discordSocketClient.ButtonExecuted += DiscordSocketClientOnButtonExecuted;

			await thread.SendMessageAsync(embed: embed.Build(), components: components.Build());
		}
	}

    private async Task DiscordSocketClientOnButtonExecuted(SocketMessageComponent component)
    {
	    switch (component.Data.CustomId)
	    {
		    case "plugin-problem-yes":
			    await component.Message.DeleteAsync();
			    await component.Channel.SendMessageAsync($"<@{prevplauserid}>, {component.User.Mention} has a problem on your plugin.");
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
    
    private async Task Ready () {
	    DiscordReady = true;
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
		if (message.Author is not SocketGuildUser member || member.IsBot)
		{
			return;
		}

		var protectedChannels = new []
		{
			_botConfig.Channels.LogChannelId,
			_botConfig.Channels.MemberScreeningChannelId,
			_botConfig.Channels.StatusCheckChannelId
		};

		if (protectedChannels.Contains(message.Channel.Id))
		{
			await message.DeleteAsync();
			return;
		}
		
		var messageByModerator = member.Roles.Contains(member.Guild.GetRole(_botConfig.Roles.ModeratorRoleId));
		var imageChannels = _botConfig.Channels.ImageOnlyChannels;


		if (message.MentionedEveryone && !messageByModerator)
		{
			await message.DeleteAsync();
			_logger.Information(
				"Message containing @everyone from {AuthorUsername}#{AuthorDiscriminator} in {ChannelName} was deleted",
				message.Author.Username,
				message.Author.Discriminator,
				message.Channel.Name);
		}
		
		if (imageChannels.Contains(message.Channel.Id) && !DiscordMessageFilter.FilterForImageChannels(message)) {
			await message.DeleteAsync();

			try {
				var embed = new EmbedBuilder() {
					Color = new Color(63, 127, 191),
					Description = message.CleanContent.Replace("<", "\\<").Replace("*", "\\*").Replace("_", "\\_").Replace("`", "\\`").Replace(":", "\\:"),
					Timestamp = message.Timestamp,
					Title = "__Your Post__",
				};
				embed.WithAuthor(member.Guild.Name, member.Guild.IconUrl);

				await member.SendMessageAsync(
					text:
					$"The channel ${message.Channel} is only meant for images.\nHere is your text, so that you don't need to rewrite it into another channel:",
					embed: embed.Build());
				_logger.Information(
					"Message without image from {AuthorUsername}#{AuthorDiscriminator} in {ChannelName} was deleted! DM with their text was successfully sent",
					message.Author.Username,
					message.Author.Discriminator,
					message.Channel.Name);

			}
			catch (HttpException) {
				_logger.Information(
					"Message without image from {AuthorUsername}#{AuthorDiscriminator} in {ChannelName} was deleted! DM with their text was not sent",
					message.Author.Username,
					message.Author.Discriminator,
					message.Channel.Name);
			}

		}
	}

	private async Task MemberMovement (SocketGuildUser member, bool joined) {
		EmbedBuilder embed = new() {
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
		var guild = _discordSocketClient.GetGuild(_botConfig.GuildId);
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

	public async Task BroadcastWebhookAsync(WebhookItem webhook, WebhookRequest webhookRequest)
	{
		if (!DiscordReady)
		{
			return;
		}
		_logger.Information("Executing Webhook {WebhookId}", webhook.Id);
		if (_discordSocketClient.GetGuild(_botConfig.GuildId)
			    .GetChannel(webhook.ChannelId) is not ITextChannel channel)
		{
			_logger.Fatal("Cannot execute webhook {WebhookId} - Channel does not exist", webhook.ChannelId);
			return;
		}

		var text = (webhookRequest.ToEveryone.HasValue && webhookRequest.ToEveryone.Value 
			? "@everyone" 
			: "᲼᲼")
			  + "\r\n"
		    + (!string.IsNullOrWhiteSpace(webhookRequest.Title) ? $"**{webhookRequest.Title}**\r\n" : "")
			  + webhookRequest.Text;

		var webhookRequestEmbed = webhookRequest.Embed;
		EmbedBuilder? embed = null;
		if (webhookRequestEmbed is not null)
		{
			_logger.Information("Webhook {WebhookId} contains an embed", webhook.Id);
			embed = new EmbedBuilder();

			if (webhookRequestEmbed.Color is not null)
			{
				var color = webhookRequestEmbed.Color;
				embed.WithColor(new Color(color.R, color.G, color.B));
			}
			
			if (!string.IsNullOrWhiteSpace(webhookRequestEmbed.Description))
			{
				embed.Description = webhookRequestEmbed.Description;
			}

			if (webhookRequestEmbed.Fields is not null && webhookRequestEmbed.Fields.Count > 0)
			{
				var embedFieldBuilders = new List<EmbedFieldBuilder>();
				foreach (var field in webhookRequestEmbed.Fields)
				{
					var embedFieldBuilder = new EmbedFieldBuilder();
					embedFieldBuilder.WithName(field.Name);
					embedFieldBuilder.WithValue(field.Value);
					embedFieldBuilder.WithIsInline(field.Inline ?? true);
					embedFieldBuilders.Add(embedFieldBuilder);
				}
				embed.WithFields(embedFieldBuilders);
			}

			if (!string.IsNullOrWhiteSpace(webhookRequestEmbed.ThumbnailUrl))
			{
				embed.ThumbnailUrl = webhookRequestEmbed.ThumbnailUrl;
			}

			if (!string.IsNullOrWhiteSpace(webhookRequestEmbed.ImageUrl))
			{
				embed.ImageUrl = webhookRequestEmbed.ImageUrl;
			}
		}

		try
		{
			await channel.SendMessageAsync(text: text, embed: embed?.Build());
			_logger.Information("Webhook {WebhookId} executed", webhook.Id);
		}
		catch (Exception ex)
		{
			_logger.Error(ex, "Cannot execute webhook {WebhookId}", webhook.Id);
		}
	}
	
	private async Task UpdateStatusMessage()
	{
		if (!DiscordReady)
		{
			return;
		}
		var status = _statusCheckService.LastStatusCheckResults.ToArray();
		var messageEmbed = DiscordStatusCheckMessageBuilder.Build(status);
		var messageEmbedHash = messageEmbed.GetHashCode();
		if (messageEmbedHash == _currentUpdateEmbedHash)
		{
			_logger.Debug(
				"Status message embed has not changed - Hash: {HashCode}",
				_currentUpdateEmbedHash);
			return;
		}
		var channel = (_discordSocketClient.GetGuild(_botConfig.GuildId)
			.GetChannel(_botConfig.Channels.StatusCheckChannelId) as ITextChannel);
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
		_currentUpdateEmbedHash = messageEmbedHash;
	}
	
}