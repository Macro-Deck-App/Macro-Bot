using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using JetBrains.Annotations;
using MacroBot.Core.Config;
using MacroBot.Core.Discord;
using MacroBot.Core.Discord.Modules.ExtensionStore;
using MacroBot.Core.Extensions;
using MacroBot.Core.Models.Webhook;
using MacroBot.Core.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using EmbedBuilderExtensions = MacroBot.Core.Extensions.EmbedBuilderExtensions;
using ILogger = Serilog.ILogger;

namespace MacroBot.Core.Services;

[UsedImplicitly]
public class DiscordService : IDiscordService, IHostedService
{
	private readonly ILogger _logger = Log.ForContext<DiscordService>();
	
	private readonly DiscordSocketClient _discordSocketClient;
	private readonly IServiceProvider _serviceProvider;
	private readonly IStatusCheckService _statusCheckService;
	private readonly InteractionService _interactionService;
	private readonly IHttpClientFactory _httpClientFactory;

	public bool DiscordReady { get; private set; }

	private ulong prevthreadid = 0;

    public DiscordService(
	    DiscordSocketClient discordSocketClient,
	    IServiceProvider serviceProvider,
	    IStatusCheckService statusCheckService,
	    InteractionService interactionService,
	    IHttpClientFactory httpClientFactory)
    {
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
		_discordSocketClient.ButtonExecuted += DiscordSocketClientOnButtonExecuted;
		_discordSocketClient.SelectMenuExecuted += DiscordSocketClientOnSelectMenuExecuted;

	    await _serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync();
	    await _discordSocketClient.LoginAsync(TokenType.Bot, MacroBotConfig.BotToken);
	    await _discordSocketClient.StartAsync();
    }

	private async Task ThreadCreated(SocketThreadChannel thread)
	{
		var msg = await thread.GetMessagesAsync(3).FlattenAsync();
		var lastMsg = msg.Last();

        var extensions = await ExtensionMessageBuilder.BuildExtensionDetectionAsync(_httpClientFactory, thread.Name, lastMsg.Content);

		if (extensions.Embeds is null || msg.First().Author.Id == _discordSocketClient.CurrentUser.Id || prevthreadid == thread.Id)
		{
			return;
		}
		
		await thread.SendMessageAsync(embed: extensions.Embeds, components: extensions.Component);
		prevthreadid = thread.Id;
	}

    private async Task DiscordSocketClientOnButtonExecuted(SocketMessageComponent component)
    {
		if (component.Data.CustomId.StartsWith("extd-no")) {
			await component.Message.DeleteAsync();
			var msg = await component.Channel.SendMessageAsync("Got it. Thank you for the clarification.");
			await Task.Delay(5000);
			await msg.DeleteAsync();
			await (component.Channel as SocketThreadChannel)!.LeaveAsync();
	    } else if (component.Data.CustomId.StartsWith("extd-")) {
			var httpClient = _httpClientFactory.CreateClient();
			var pl = await httpClient.GetExtensionAsync(component.Data.CustomId.Remove("extd-"));
			await component.Message.DeleteAsync();
			var channel = (component.Channel as IThreadChannel)!;
			await channel.ModifyAsync(x => x.Name = $"{channel.Name} (Extension Problem: {pl!.Name})");
			await (component.Channel as SocketThreadChannel)!.LeaveAsync();
		}
    }

	private async Task DiscordSocketClientOnSelectMenuExecuted(SocketMessageComponent component) {
		if (component.Data.CustomId.StartsWith("extd-selmenu")) {
			if (component.Data.Values.Count > 1)
			{
				await component.DeferAsync();
				var httpClient = _httpClientFactory.CreateClient();
				List<EmbedFieldBuilder> embedFieldBuilders = new();
				var channel = (component.Channel as IThreadChannel)!;
				foreach (var val in component.Data.Values) {
					var pl = await httpClient.GetExtensionAsync(val.Remove("extd-"));
					embedFieldBuilders.Add(new EmbedFieldBuilder() {
						Name = pl!.Name,
						Value = $"by **{pl.Author}**",
						IsInline = true
					});
				}
				await channel.ModifyAsync(x => x.Name = $"{channel.Name} ({component.Data.Values.Count()} extensions)");
				await component.Message.ModifyAsync(x => {
					x.Embed = new EmbedBuilder() {
						Title = "This user has a problem on these extensions"
					}.WithFields(embedFieldBuilders).Build();
					x.Components = null;
				});
				await (component.Channel as SocketThreadChannel)!.LeaveAsync();
			} else {
				var httpClient = _httpClientFactory.CreateClient();
				var pl = await httpClient.GetExtensionAsync(component.Data.Values.ToList()[0].Remove("extd-"));
				await component.Message.DeleteAsync();
				var channel = (component.Channel as IThreadChannel)!;
				await channel.ModifyAsync(x => x.Name = $"{channel.Name} (Extension Problem: {pl!.Name})");
				await (component.Channel as SocketThreadChannel)!.LeaveAsync();
			}
		}
	}
    
    private async Task Ready ()
    {
	    DiscordReady = true;
	    await _interactionService.RegisterCommandsGloballyAsync();
	    var guild = _discordSocketClient.GetGuild(MacroBotConfig.GuildId);
	    if (guild?.GetChannel(MacroBotConfig.MemberScreeningChannelId) is ITextChannel channel)
	    {
		    var usersCount = GetUsersCount(guild);
		    await UpdateMemberScreeningChannelName(channel, usersCount);
	    }
    }

	private async Task UserJoined (SocketGuildUser member)
	{
		await MemberMovement(member, true);
	}

	private async Task UserLeft (SocketGuild guild, SocketUser user)
	{
		if (user is SocketGuildUser guildUser)
		{
			await MemberMovement(guildUser, false);
		}
	}

	private async Task MessageReceived (SocketMessage message)
	{
		if (message.Author is not SocketGuildUser member || member.IsBot)
		{
			return;
		}

		var protectedChannels = new []
		{
			MacroBotConfig.LogChannelId,
			MacroBotConfig.MemberScreeningChannelId,
			MacroBotConfig.StatusCheckChannelId
		};

		if (protectedChannels.Contains(message.Channel.Id))
		{
			await message.DeleteAsync();
			return;
		}
		
		var messageByModerator = member.Roles.Contains(member.Guild.GetRole(MacroBotConfig.ModeratorRoleId));
		var imageChannels = MacroBotConfig.ImageOnlyChannelIds;

		var anyMentionsOnMsg = message.MentionedUsers.Any(u => message.Content.Contains($"<@{u.Id}>"));

		if ((message.MentionedEveryone 
		     || message.MentionedRoles.Count > 0
		     || (message.MentionedUsers.Count > 0 && (anyMentionsOnMsg || message.Type != MessageType.Reply))
		    && !(messageByModerator || member.GetPermissions(message.Channel as IGuildChannel).ManageMessages)
			&& !(message.MentionedUsers.Count == 1 && message.Content.Contains($"<@{message.Author.Id}>"))))
		{
			await message.DeleteAsync();
			_logger.Information(
				"Message containing users/roles/everyone from {AuthorUsername} in {ChannelName} was deleted. Message: {Message}",
				message.Author.Username,
				message.Channel.Name,
				message.CleanContent);
			try
			{
				await member.SendMessageAsync(embed: new EmbedBuilder
				{
					Title = "Hello there!",
					Description = $"It looks like you mentioned either a user, a role or @everyone! It is not allowed on the {(message.Channel as IGuildChannel)!.Guild.Name} server. Your message is below."
				}.AddField("Message", message.CleanContent).Build());
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Cannot send message to {User}", message.Author.Username);
			}
		}

		if (imageChannels.Contains(message.Channel.Id) && !DiscordMessageFilter.FilterForImageChannels(message))
		{
			await message.DeleteAsync();

			try
			{
				var embed = new EmbedBuilder
				{
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
					"Message without image from {AuthorUsername} in {ChannelName} was deleted! DM with their text was successfully sent",
					message.Author.Username,
					message.Channel.Name);

			}
			catch (HttpException)
			{
				_logger.Information(
					"Message without image from {AuthorUsername} in {ChannelName} was deleted! DM with their text was not sent",
					message.Author.Username,
					message.Channel.Name);
			}

		}
	}

	private async Task MemberMovement(IGuildUser member, bool joined)
	{
		var guild = _discordSocketClient.GetGuild(MacroBotConfig.GuildId);
		if (guild?.GetChannel(MacroBotConfig.MemberScreeningChannelId) is not ITextChannel channel)
		{
			return;
		}
		_logger.Verbose("{User} {Action} the server",
			member.Username,
			joined
				? "joined"
				: "left");

		var usersCount = GetUsersCount(guild);
		var botsCount = GetBotsCount(guild);
		await UpdateMemberScreeningChannelName(channel, usersCount);
		EmbedBuilder embed = new()
		{
			Color = joined ? Color.Green : Color.Red,
			Description = $"Latest member count: **{usersCount}** ({botsCount} bots)",
			ThumbnailUrl = member.GetAvatarUrl(),
			Title = $"__**{member.Username} {(joined ? "joined" : "left")} the server!**__"
		};
		embed.WithCurrentTimestamp();

		embed.AddField("__ID__", member.Id, member.Nickname != null);
		if (member.Nickname != null)
		{
			embed.AddField("__Nickname__", member.Nickname, true);
			EmbedBuilderExtensions.AddBlankField(embed, true);
		}
		embed.AddField("__Joined__", $"<t:{member.JoinedAt?.ToUnixTimeSeconds()}:R>", true);
		embed.AddField("__Created__", $"<t:{member.CreatedAt.ToUnixTimeSeconds()}:R>", true);
		await channel.SendMessageAsync(embed: embed.Build());
	}

	private async Task UpdateMemberScreeningChannelName (ITextChannel channel, int userCount)
	{
		// Temporarily disabled because of a rate limit
		return;
		if (channel is not SocketGuildChannel socketGuildChannel)
		{
			return;
		}
		await socketGuildChannel.ModifyAsync(properties =>
		{
			properties.Name = $"{userCount}_users";
		});
	}

	private int GetUsersCount(SocketGuild guild)
	{
		return guild.Users.ToArray().Length;
	}

	private int GetBotsCount(SocketGuild guild)
	{
		return guild.Users.Where(x => x.IsBot).ToArray().Length;
	}

	public async Task BroadcastWebhookAsync(WebhookItem webhook, WebhookRequest webhookRequest)
	{
		if (!DiscordReady)
		{
			return;
		}
		_logger.Verbose("Executing Webhook {WebhookId}", webhook.Id);
		if (_discordSocketClient.GetGuild(MacroBotConfig.GuildId)
			    .GetChannel(webhook.ChannelId) is not ITextChannel channel)
		{
			_logger.Fatal("Cannot execute webhook {WebhookId} - Channel does not exist", webhook.ChannelId);
			return;
		}

		var text = (string?)null;
		if (!string.IsNullOrWhiteSpace(webhookRequest.Title) 
		    || webhookRequest.ToEveryone.GetValueOrDefault())
		{
			text = (webhookRequest.ToEveryone.HasValue && webhookRequest.ToEveryone.Value 
				       ? "@everyone" 
				       : "")
			       + "\r\n"
			       + (!string.IsNullOrWhiteSpace(webhookRequest.Title) ? $"**{webhookRequest.Title}**\r\n" : "")
			       + webhookRequest.Text;
		}

		var webhookRequestEmbed = webhookRequest.Embed;
		EmbedBuilder? embed = null;
		if (webhookRequestEmbed is not null)
		{
			_logger.Verbose("Webhook {WebhookId} contains an embed", webhook.Id);
			embed = new EmbedBuilder();

			if (webhookRequestEmbed.Color is not null)
			{
				var color = webhookRequestEmbed.Color;
				embed.WithColor(new Color(color.R, color.G, color.B));
			}

			if (!string.IsNullOrWhiteSpace(webhookRequestEmbed.Title))
			{
				embed.Title = webhookRequestEmbed.Title;
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
			_logger.Verbose("Webhook {WebhookId} executed", webhook.Id);
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
		var status = _statusCheckService.LastStatusCheckResults?.ToArray();

		if (_discordSocketClient.GetGuild(MacroBotConfig.GuildId)
			    .GetChannel(MacroBotConfig.StatusCheckChannelId) is not ITextChannel channel 
		    || status is null)
		{
			return;
		}
		
		
		var messageEmbed = DiscordStatusCheckMessageBuilder.Build(status);

		try
		{
			await channel.SendMessageAsync(embed: messageEmbed);
		}
		catch (Exception ex)
		{
			_logger.Error(ex, "Cannot send status update");
		}
	}
	
}