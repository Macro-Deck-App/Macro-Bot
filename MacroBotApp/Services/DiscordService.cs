using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Web;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using JetBrains.Annotations;
using MacroBot.Config;
using MacroBot.Discord;
using MacroBot.Discord.Modules.ExtensionStore;
using MacroBot.Extensions;
using MacroBot.Models.Extensions;
using MacroBot.Models.Translate;
using MacroBot.Models.Webhook;
using MacroBot.ServiceInterfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroBot.Services;

[UsedImplicitly]
public class DiscordService : IDiscordService, IHostedService
{
    private readonly BotConfig _botConfig;
    private readonly CommandsConfig _commandsConfig;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly ExtensionDetectionConfig _extDetectionConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly InteractionService _interactionService;
    private readonly ILogger _logger = Log.ForContext<DiscordService>();
    private readonly IServiceProvider _serviceProvider;
    private readonly IStatusCheckService _statusCheckService;

    private ulong _previousThreadId;

    public DiscordService(BotConfig botConfig,
        CommandsConfig commandsConfig,
        ExtensionDetectionConfig extDetectionConfig,
        DiscordSocketClient discordSocketClient,
        IServiceProvider serviceProvider,
        IStatusCheckService statusCheckService,
        InteractionService interactionService,
        IHttpClientFactory httpClientFactory)
    {
        _botConfig = botConfig;
        _commandsConfig = commandsConfig;
        _extDetectionConfig = extDetectionConfig;
        _discordSocketClient = discordSocketClient;
        _serviceProvider = serviceProvider;
        _statusCheckService = statusCheckService;
        _interactionService = interactionService;
        _httpClientFactory = httpClientFactory;
    }

    public bool DiscordReady { get; private set; }

    public async Task BroadcastWebhookAsync(WebhookItem webhook, WebhookRequest webhookRequest)
    {
        if (!DiscordReady) return;
        _logger.Verbose("Executing Webhook {WebhookId}", webhook.Id);
        if (_discordSocketClient.GetGuild(_botConfig.GuildId)
                .GetChannel(webhook.ChannelId) is not ITextChannel channel)
        {
            _logger.Fatal("Cannot execute webhook {WebhookId} - Channel does not exist", webhook.ChannelId);
            return;
        }

        var text = (string?)null;
        if (!string.IsNullOrWhiteSpace(webhookRequest.Title)
            || webhookRequest.ToEveryone.GetValueOrDefault())
            text = (webhookRequest.ToEveryone.HasValue && webhookRequest.ToEveryone.Value
                       ? "@everyone"
                       : string.Empty)
                   + "\r\n"
                   + (!string.IsNullOrWhiteSpace(webhookRequest.Title)
                       ? $"**{webhookRequest.Title}**\r\n"
                       : string.Empty)
                   + webhookRequest.Text;

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

            if (!string.IsNullOrWhiteSpace(webhookRequestEmbed.Title)) embed.Title = webhookRequestEmbed.Title;

            if (!string.IsNullOrWhiteSpace(webhookRequestEmbed.Description))
                embed.Description = webhookRequestEmbed.Description;

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
                embed.ThumbnailUrl = webhookRequestEmbed.ThumbnailUrl;

            if (!string.IsNullOrWhiteSpace(webhookRequestEmbed.ImageUrl)) embed.ImageUrl = webhookRequestEmbed.ImageUrl;
        }

        try
        {
            await channel.SendMessageAsync(text, embed: embed?.Build());
            _logger.Verbose("Webhook {WebhookId} executed", webhook.Id);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Cannot execute webhook {WebhookId}", webhook.Id);
        }
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
        _discordSocketClient.ThreadCreated += async thread => await ThreadCreated(thread);
        _discordSocketClient.SelectMenuExecuted += DiscordSocketClientOnSelectMenuExecuted;
        _discordSocketClient.ButtonExecuted += DiscordSocketClientOnButtonExecuted;

        await _serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync();
        await _discordSocketClient.LoginAsync(TokenType.Bot, _botConfig.Token);
        await _discordSocketClient.StartAsync();
    }

    private async Task ThreadCreated(SocketThreadChannel thread)
    {
        if (thread.Id == _previousThreadId) return;
        _previousThreadId = thread.Id;

        var messages = await thread.GetMessagesAsync(2).FlattenAsync();
        messages = messages?.ToArray();

        var firstMessage = messages?.FirstOrDefault();
        if (firstMessage == null || firstMessage.Author.IsBot) return;

        var lastMessage = messages?.Last();

        using var httpClient = _httpClientFactory.CreateClient();
        var extensions = await httpClient.GetFromJsonAsync<ExtensionResponse>(
            $"{_extDetectionConfig.AllExtensionsUrl}?ItemsPerPage={_extDetectionConfig.ExtensionsPerPage}");

        var extensionsList = new List<AllExtensions>();
        if (extensions?.TotalItemsCount > 0 && extensions.Data != null)
            foreach (var extension in extensions.Data)
            {
                var splitName = extension.Name.Split(" ");
                var contentSplit = lastMessage?.CleanContent.Remove("Plugin").Remove("plugin").Replace("\r\n", " ")
                    .Split(" ");
                var nameSplit = thread.Name.Remove("Plugin").Remove("plugin").Replace("\r\n", " ")
                    .Split(" ");
                if (splitName.Any(sN => (contentSplit != null &&
                                         contentSplit.Contains(sN, StringComparer.CurrentCultureIgnoreCase))
                                        || nameSplit.Contains(sN, StringComparer.CurrentCultureIgnoreCase)))
                    extensionsList.Add(extension);
            }

        if (extensionsList.Count <= 0) return;

        await thread.SendMessageAsync(embed: ExtensionMessageBuilder.BuildProblemExtensionAsync(extensionsList),
            components: ExtensionMessageBuilder.BuildProblemExtensionInteractionAsync(extensionsList));
    }

    private async Task DiscordSocketClientOnButtonExecuted(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "ProblemExtensionButtonNo":
                await component.DeferAsync();
                await component.Message.DeleteAsync();
                var msg = await component.Channel.SendMessageAsync("Thanks for the clarification.");
                await Task.Delay(5000);
                await msg.DeleteAsync();
                break;
            default:
                return;
        }
    }

    private async Task DiscordSocketClientOnSelectMenuExecuted(SocketMessageComponent component)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        switch (component.Data.CustomId)
        {
            case "ProblemExtensionInteraction":
                try
                {
                    var embed = new EmbedBuilder
                    {
                        Title = "This thread has problems on these extensions or icon packs."
                    };
                    foreach (var str in component.Data.Values)
                    {
                        var extension =
                            await httpClient.GetFromJsonAsync<Extension>(
                                $"{_extDetectionConfig.AllExtensionsUrl}/{HttpUtility.UrlEncode(str)}");

                        var a = extension.DSupportUserId is null
                            ? extension.Author
                            : string.Format("<@{UserId}>", extension.DSupportUserId);
                        var desc = extension.Description.IsNullOrWhiteSpace()
                            ? string.Empty
                            : $"\r\n{extension.Description}";
                        embed.AddField(extension.Name, $"by {a}{desc}", true);
                    }

                    await component.Channel.SendMessageAsync(embed: embed.Build());
                    await component.Message.DeleteAsync();
                }
                catch (Exception e)
                {
                    _logger.Warning(e, "Can't create embed!");
                }

                break;
            default:
                return;
        }
    }

    private async Task Ready()
    {
        DiscordReady = true;
        try
        {
            await _interactionService.RegisterCommandsGloballyAsync();
        }
        catch (Exception e)
        {
            _logger.Error(e, "There is an error while registering commands!");
        }

        var guild = _discordSocketClient.GetGuild(_botConfig.GuildId);
        if (guild?.GetChannel(_botConfig.Channels.MemberScreeningChannelId) is ITextChannel channel)
        {
            var usersCount = GetUsersCount(guild);
            await UpdateMemberScreeningChannelName(channel, usersCount);
        }

        _logger.Information("Bot ready");
    }

    private async Task UserJoined(SocketGuildUser member)
    {
        await MemberMovement(member, true);
    }

    private async Task UserLeft(SocketGuild guild, SocketUser user)
    {
        if (user is SocketGuildUser guildUser) await MemberMovement(guildUser, false);
    }

    private async Task MessageReceived(SocketMessage message)
    {
        if (message.Author is not SocketGuildUser user || user.IsBot) return;

        var protectedChannels = new[]
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

        var messageByAdministrator = user.Roles.Contains(user.Guild.GetRole(_botConfig.Roles.AdministratorRoleId));
        var messageByModerator = user.Roles.Contains(user.Guild.GetRole(_botConfig.Roles.ModeratorRoleId));
        var messageByDevTeamMember = user.Roles.Contains(user.Guild.GetRole(_botConfig.Roles.DevTeamRoleId));
        var messageByCommunityDeveloper =
            user.Roles.Contains(user.Guild.GetRole(_botConfig.Roles.CommunityDeveloperRoleId));
        var imageChannels = _botConfig.Channels.ImageOnlyChannels;

        if ((message.MentionedEveryone
             || message.MentionedRoles.Count > 0
             || message.MentionedUsers.Count > 0)
            && !messageByAdministrator
            && !messageByModerator
            && !messageByDevTeamMember
            && !(messageByCommunityDeveloper && message.Channel.Id == _botConfig.Channels.PluginAnnouncementsChannelId)
            && message.Type != MessageType.Reply
            && !(message.MentionedUsers.Count == 1 && message.Content.Contains($"<@{message.Author.Id}>")))
        {
            await message.DeleteAsync();

            _logger.Information(
                "Message that contains either a user, a role, everyone or here is detected.\r\n" +
                "By {User} in {Channel}, Message:\r\n{Content}",
                user.DisplayName, message.Channel, message.Content);

            try
            {
                await user.SendMessageAsync(embed: new EmbedBuilder
                {
                    Title = "Hey there!",
                    Description =
                        $"It looks like you mentioned someone, a role, or @everyone! It is not allowed on the {(message.Channel as IGuildChannel).Guild.Name} server."
                }.AddField("Message", message.Content).Build());
            }
            catch (Exception e)
            {
                _logger.Error(e, "Can't send a message to {User}. Maybe DMs disabled?", message.Author.Username);
            }
        }
        else if (imageChannels.Contains(message.Channel.Id) && !DiscordMessageFilter.FilterForImageChannels(message))
        {
            await message.DeleteAsync();
            if (imageChannels.Contains(message.Channel.Id) && !DiscordMessageFilter.FilterForImageChannels(message))
            {
                await message.DeleteAsync();

                try
                {
                    var embed = new EmbedBuilder
                    {
                        Color = new Color(63, 127, 191),
                        Description = message.CleanContent.Replace("<", "\\<").Replace("*", "\\*").Replace("_", "\\_")
                            .Replace("`", "\\`").Replace(":", "\\:"),
                        Timestamp = message.Timestamp,
                        Title = "__Your Post__"
                    };
                    embed.WithAuthor(user.Guild.Name, user.Guild.IconUrl);

                    await user.SendMessageAsync(
                        $"The channel ${message.Channel} is only meant for images.\nHere is your text, so that you don't need to rewrite it into another channel:",
                        embed: embed.Build());
                    _logger.Information(
                        "Message without image from {AuthorUsername}#{AuthorDiscriminator} in {ChannelName} was deleted! DM with their text was successfully sent",
                        message.Author.Username,
                        message.Author.Discriminator,
                        message.Channel.Name);
                }
                catch (HttpException)
                {
                    _logger.Information(
                        "Message without image from {AuthorUsername}#{AuthorDiscriminator} in {ChannelName} was deleted! DM with their text was not sent",
                        message.Author.Username,
                        message.Author.Discriminator,
                        message.Channel.Name);
                }
            }
        }

        using var httpClient = _httpClientFactory.CreateClient();
        var dict = new Dictionary<string, string>();
        dict.Add("q", message.Content);
        dict.Add("source", "auto");
        var username = _commandsConfig.Translate.UserName;
        var password = _commandsConfig.Translate.Password;
        var svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
        dict.Add("target", "en");
        var request = new HttpRequestMessage(HttpMethod.Post, _commandsConfig.Translate.Url)
        {
            Content = new FormUrlEncodedContent(dict)
        };
        request.Headers.Add("Authorization", "Basic " + svcCredentials);
        var result = await httpClient.SendAsync(request);
        var translated = JsonSerializer.Deserialize<Translated>(await result.Content.ReadAsStringAsync());

        if (translated.DetectedLanguage.Language != "en")
        {
            var displayName = string.Empty;
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach (var culture in cultures)
            {
                // Exclude custom cultures.
                if ((culture.CultureTypes & CultureTypes.UserCustomCulture) == CultureTypes.UserCustomCulture)
                    continue;

                // Exclude all three-letter codes.
                if (culture.TwoLetterISOLanguageName.Length == 3)
                    continue;

                if (culture.TwoLetterISOLanguageName.Contains(translated.DetectedLanguage.Language,
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    displayName = culture.DisplayName;
                    break;
                }
            }

            message.Channel.SendMessageAsync(user.Mention, embed: new EmbedBuilder
                {
                    Title = "Macro Bot Translate System",
                    Description = "We detected that your message is not on English, so we translated it!"
                }.AddField("Detected Language", displayName, true)
                .AddField("Confidence", translated.DetectedLanguage.Confidence, true)
                .AddField("Text", translated.TranslatedText).Build());
        }
    }

    private async Task MemberMovement(IGuildUser member, bool joined)
    {
        var guild = _discordSocketClient.GetGuild(_botConfig.GuildId);
        if (guild?.GetChannel(_botConfig.Channels.MemberScreeningChannelId) is not ITextChannel channel) return;
        _logger.Verbose("{User}#{Discriminator} {Action} the server",
            member.Username,
            member.Discriminator,
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
            Title = $"__**{member.Username}#{member.Discriminator} {(joined ? "joined" : "left")} the server!**__"
        };
        embed.WithCurrentTimestamp();

        embed.AddField("__ID__", member.Id, member.Nickname != null);
        if (member.Nickname != null)
        {
            embed.AddField("__Nickname__", member.Nickname, true);
            embed.AddBlankField(true);
        }

        embed.AddField("__Joined__", $"<t:{member.JoinedAt?.ToUnixTimeSeconds()}:R>", true);
        embed.AddField("__Created__", $"<t:{member.CreatedAt.ToUnixTimeSeconds()}:R>", true);
        await channel.SendMessageAsync(embed: embed.Build());
    }

    private async Task UpdateMemberScreeningChannelName(ITextChannel channel, int userCount)
    {
        // Temporarily disabled because of a rate limit
        return;
        if (channel is not SocketGuildChannel socketGuildChannel) return;
        await socketGuildChannel.ModifyAsync(properties => { properties.Name = $"{userCount}_users"; });
    }

    private int GetUsersCount(SocketGuild guild)
    {
        var userCount = guild.Users.ToArray().Length;
        return userCount;
    }

    private int GetBotsCount(SocketGuild guild)
    {
        var botCount = guild.Users.Where(x => x.IsBot).ToArray().Length;
        return botCount;
    }

    private async Task UpdateStatusMessage()
    {
        if (!DiscordReady) return;
        var status = _statusCheckService.LastStatusCheckResults?.ToArray();

        if (_discordSocketClient.GetGuild(_botConfig.GuildId)
                .GetChannel(_botConfig.Channels.StatusCheckChannelId) is not ITextChannel channel
            || status is null)
            return;


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