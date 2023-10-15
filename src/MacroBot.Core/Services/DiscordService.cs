using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using JetBrains.Annotations;
using MacroBot.Core.Config;
using MacroBot.Core.DataAccess.Entities;
using MacroBot.Core.DataAccess.RepositoryInterfaces;
using MacroBot.Core.Discord;
using MacroBot.Core.Discord.Modules.OldExtensionStore;
using MacroBot.Core.Extensions;
using MacroBot.Core.Models.Webhook;
using MacroBot.Core.ServiceInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCalc;
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
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public bool DiscordReady { get; private set; }

	private string prevthread = "";
    private ulong? prevplauserid = 0;
    private string prevplugin = "";
	private List<string> plsinthisthread = new();

    public DiscordService(
	    DiscordSocketClient discordSocketClient,
	    IServiceProvider serviceProvider,
	    IStatusCheckService statusCheckService,
	    InteractionService interactionService,
	    IHttpClientFactory httpClientFactory,
		IServiceScopeFactory serviceScopeFactory)
    {
	    _discordSocketClient = discordSocketClient;
	    _serviceProvider = serviceProvider;
	    _statusCheckService = statusCheckService;
	    _interactionService = interactionService;
	    _httpClientFactory = httpClientFactory;
		_serviceScopeFactory = serviceScopeFactory;
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

	    await _serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync();
	    await _discordSocketClient.LoginAsync(TokenType.Bot, MacroBotConfig.BotToken);
	    await _discordSocketClient.StartAsync();
    }

	private async Task ThreadCreated(SocketThreadChannel thread)
	{
		plsinthisthread = new();
		var msg = await thread.GetMessagesAsync(2).FlattenAsync();
		var lastMsg = msg.Last();
		PluginUtils pluginUtils = new PluginUtils();
        var extensions = await pluginUtils.GetPluginsAsync();

		if (extensions is null)
		{
			return;
		}
		
		foreach (var extension in extensions)
		{
			if (lastMsg.CleanContent.IndexOf(extension.Name.Replace(" Plugin", ""), StringComparison.OrdinalIgnoreCase) < 0 &&
			    lastMsg.CleanContent.IndexOf(extension.PackageId, StringComparison.OrdinalIgnoreCase) < 0 &&
			    thread.Name.IndexOf(extension.Name.Replace(" Plugin", ""), StringComparison.OrdinalIgnoreCase) < 0 &&
			    thread.Name.IndexOf(extension.PackageId, StringComparison.OrdinalIgnoreCase) < 0) continue;

			if ((prevthread == thread.Name && prevplugin == extension.PackageId) || (plsinthisthread.Contains(extension.PackageId))) return;
			if (extension.Type.IndexOf("plugin", StringComparison.OrdinalIgnoreCase) < 0) continue;

			plsinthisthread.Add(extension.PackageId);
			
			var embed = new EmbedBuilder
			{
				Title = $"Do you have a problem with a plugin?",
				Description = $"Macro Bot detects a plugin name on your post.\r\nIf your problem is this plugin, click Yes. Otherwise, click No."
			};

			embed.AddField("Name", $"{extension.Name} ({extension.PackageId})", true);
			embed.AddField("Author", extension.Author, true);
			prevthread = thread.Name;
			prevplugin = extension.PackageId!;

			var components = new ComponentBuilder()
				.WithButton("Yes", "plugin-problem-yes", ButtonStyle.Success)
				.WithButton("No", "plugin-problem-no", ButtonStyle.Danger);

			_discordSocketClient.ButtonExecuted -= DiscordSocketClientOnButtonExecuted;
			_discordSocketClient.ButtonExecuted += DiscordSocketClientOnButtonExecuted;

			await thread.SendMessageAsync(embed: embed.Build(), components: components.Build());
		}
	}

	/*
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
			if (_prevThread == thread.Name)
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
			_prevThread = thread.Name;
			_prevPlaUserId = extension.DSupportUserId is not null ? ulong.Parse(extension.DSupportUserId) : null;
			_prevPlugin = extension.PackageId!;

			var components = new ComponentBuilder()
				.WithButton("Yes", "plugin-problem-yes", ButtonStyle.Success)
				.WithButton("No", "plugin-problem-no", ButtonStyle.Danger);

			_discordSocketClient.ButtonExecuted -= DiscordSocketClientOnButtonExecuted;
			_discordSocketClient.ButtonExecuted += DiscordSocketClientOnButtonExecuted;

			await thread.SendMessageAsync(embed: embed.Build(), components: components.Build());
		}
	}
	*/

    private async Task DiscordSocketClientOnButtonExecuted(SocketMessageComponent component)
    {
	    switch (component.Data.CustomId)
	    {
		    case "plugin-problem-yes":
			    await component.Message.DeleteAsync();
			   // await component.Channel.SendMessageAsync($"{component.User.Mention} has a problem on your plugin."); // <@{prevplauserid}>, 
			    await (component.Channel as SocketThreadChannel)!.ModifyAsync(msg => msg.Name = @$"{component.Channel.Name} (Plugin Problem - {prevplugin})");
			    await (component.Channel as SocketThreadChannel)!.LeaveAsync();
				plsinthisthread = new();
			    break;
		    case "plugin-problem-no":
			    await component.Message.DeleteAsync();
			    var msg = await component.Channel.SendMessageAsync("Got it. Thank you for the clarification.");
			    await Task.Delay(5000);
			    await msg.DeleteAsync();
			    await (component.Channel as SocketThreadChannel)!.LeaveAsync();
				plsinthisthread = new();
			    break;
		    default:
			    return;
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

		if (message.Channel.Id == MacroBotConfig.CountingChannelId)
		{
			await using var scope = _serviceScopeFactory.CreateAsyncScope();
        	var countingRepository = scope.ServiceProvider.GetRequiredService<ICountingRepository>();

			try
			{
				var expr = new Expression(message.CleanContent)
				.Evaluate();

				if (expr is IConvertible conv)
				{
					var cnt = await countingRepository.GetCurrentCount();
					var e = conv.ToInt64(null) == cnt!.CurrentCount + 1;
					if (e)
					{
						if (cnt.CurrentAuthor != message.Author.Id)
						{
							await message.AddReactionAsync(conv.ToInt64(null) >= cnt.HighScore? new Emoji("☑️") : new Emoji("✅"));

							await countingRepository.SetCount(conv.ToInt64(null), message.Author.Id);
							return;
						}
						await CountingRuined(message as IUserMessage, conv.ToInt64(null), cnt, string.Format("You can't count more than once.", cnt.CurrentCount++, conv.ToInt64(null)));
						return;
					}

					await CountingRuined(message as IUserMessage, conv.ToInt64(null), cnt, string.Format("{0} is not {1}.", conv.ToInt64(null), cnt.CurrentCount + 1));
				}
			}
			catch
			{ 
				// It is ignored.
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

	private async Task CountingRuined(IUserMessage message, long count, CountingEntity currentCount, string reason)
	{
		await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var countingRepository = scope.ServiceProvider.GetRequiredService<ICountingRepository>();

		await message.AddReactionAsync(new Emoji("❌"));

		EmbedBuilder embedBuilder = new()
		{
			Title = "Someone ruined it! Start again at 1!",
			Description = reason
		};

		embedBuilder.AddField("Ruined by", message.Author.Mention, true);
		embedBuilder.AddField("Original Message", message.Content, true);

		await countingRepository.SetCount(0, 0);
		if (currentCount.CurrentCount >= currentCount.HighScore)
		{
			await countingRepository.SetCountHighScore(currentCount.CurrentCount);
			embedBuilder.AddField("New High Score!", currentCount.CurrentCount, true);
		}

		await message.ReplyAsync(embed: embedBuilder.Build());
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