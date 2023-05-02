using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using JetBrains.Annotations;

namespace MacroBot.Discord.Modules.Info;

[Group("info", "Show information about the bot, server, channel, or user")]
[UsedImplicitly]
public class BotInfoModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("bot", "Show information about the bot")]
    public async Task Bot()
    {
        EmbedBuilder embed = new()
        {
            Title = Context.Client.CurrentUser.Username,
            Description = "Here you can find the information of this Discord Bot."
        };
        embed.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());
        embed.AddField("Description", "This is the official Discord Bot for the Macro Deck discord server by <@!300244123569487873>.");
        embed.AddField("Usage", "This bot functions using Discord's Slash Command feature, type `/` in chat in order to view a list of valid commands.");
        embed.AddField("Version", $"`{Assembly.GetExecutingAssembly().GetName().Version}`", true);
        embed.AddField("API Latency", $"`{Context.Client.Latency} ms`", true);
        embed.AddField("Written by", "<@!298215920709664768>\n<@!367398650076463118>\n<@!908002848967626842>\n<@!300244123569487873>", true);
        embed.WithFooter(Context.Guild.Name + " | " + Context.Guild.Id, Context.Guild.IconUrl);

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    [UserCommand("Show user information")]
    public async Task userInfoUC(SocketGuildUser user)
    {
        await userInfo(user);
    }

    [SlashCommand("user", "Show information about a user")]
    public async Task userInfo([Summary(description: "Shows information about the user")] SocketGuildUser user, bool asUser = false)
    {
        EmbedBuilder embed = new();
        if (user.IsBot != true || asUser)
        {
            embed.Title = user.Username;
            embed.Description = "Here you can find the information of " + $"<@!{user.Id}>" + ".";
            embed.WithThumbnailUrl(Context.Client.GetUser(user.Id).GetAvatarUrl());
            embed.AddField("Status", Context.Client.GetUser(user.Id).Status, true);
            var acts = "";
            try
            {  
                foreach (var activities in user.Activities)
                {
                    if (activities is SpotifyGame spot)
                    {
                        acts += $"[Spotify] [**{spot.TrackTitle}**]({spot.TrackUrl}) {string.Join(", ", spot.Artists)} ({spot.Elapsed.Value.ToString(@"hh\:mm\:ss")} / {spot.Duration.Value.ToString(@"hh\:mm\:ss")})\r\n";
                    }
                    else if (activities is CustomStatusGame customStatus)
                    {
                        acts += $"[{customStatus.Name}] **{customStatus.Emote.Name} {customStatus.Details}**\r\n";
                    }
                    else if (activities is RichGame richGame)
                    {
                        acts += $"[{richGame.Name}]{(!string.IsNullOrEmpty(richGame.Details)? $" **{richGame.Details}**" : " No details")}\r\n";
                    }
                    else
                    {
                        acts += $"[{activities.Type}] **{activities.Name}** {(!string.IsNullOrEmpty(activities.Details)? $" {activities.Details}" : " No details")}\r\n";
                    }
                }
            }
            catch
            {
                // ignored
            }

            embed.AddField("Activities", (acts == "")? "None" : acts);
            embed.AddField("Roles", string.Join(", ", Context.Guild.GetUser(user.Id).Roles.Select(r => r.Mention)), true);
            embed.AddField("Creation Date", $"<t:{Context.Client.GetUser(user.Id).CreatedAt.ToUnixTimeSeconds()}>");
            embed.AddField("Join Date", $"<t:{Context.Guild.GetUser(user.Id).JoinedAt.Value.ToUnixTimeSeconds()}>");
            try
            {
                embed.AddField("Active Clients", String.Join(", ", user.ActiveClients));
            }
            catch
            {
                // ignored
            }
            embed.WithFooter("ID: " + Context.Client.GetUser(user.Id).Id);
        }
        else
        {
            embed.Title = user.Username;
            embed.Description = "Here you can find the information of the bot " + $"<@!{user.Id}>" + ".";
            embed.WithThumbnailUrl(Context.Client.GetUser(user.Id).GetAvatarUrl());
            embed.AddField("Status", Context.Client.GetUser(user.Id).Status, true);
            embed.AddField("Creation Date", $"<t:{Context.Client.GetUser(user.Id).CreatedAt.ToUnixTimeSeconds()}>");
            embed.AddField("Join Date", $"<t:{Context.Guild.GetUser(user.Id).JoinedAt.Value.ToUnixTimeSeconds()}>");
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

        if (user.IsBot == true)
        {
            embed.WithColor(new Color(88, 101, 242));
        }

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    [SlashCommand("guild", "Show information about the guild (same as /info server)")]
    public async Task GuildInfo()
    {
        EmbedBuilder embed = new()
        {
            Title = Context.Guild.Name,
            Description = "Here you can find the information of this server."
        };
        embed.WithThumbnailUrl(Context.Guild.IconUrl);
        embed.AddField("Owner", "<@" + Context.Guild.OwnerId + ">");
        string member;
        var epm = false;
        if (Context.Guild.MemberCount > 25)
        {
            member = Context.Guild.MemberCount.ToString();
            epm = true;
        }
        else
        {
            member = string.Join(", ", Context.Guild.Users.Select(u => u.Mention));
            epm = false;
        }
        embed.AddField("Members", member, epm);
        string text;
        if (Context.Guild.TextChannels.Count > 10)
        {
            text = Context.Guild.TextChannels.Count.ToString();
        }
        else if (Context.Guild.TextChannels.Count < 1)
        {
            text = "__**No text channels found**__";
        }
        else
        {
            text = string.Join(", ", Context.Guild.TextChannels.Select(u => u.Mention));
        }
        embed.AddField("Text Channels", string.Join(", ", Context.Guild.TextChannels.Select(u => u.Mention)), true);
        string voice;
        if (Context.Guild.VoiceChannels.Count > 10) 
        {
            voice = Context.Guild.VoiceChannels.Count.ToString();
        }
        else if (Context.Guild.VoiceChannels.Count < 1)
        {
            voice = "__**No voice channels found**__";
        }
        else
        {
            voice = string.Join(", ", Context.Guild.VoiceChannels.Select(u => u.Mention));
        }
        embed.AddField("Voice Channels", voice, true);
        embed.WithFooter($"ID: {Context.Guild.Id}");

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    [SlashCommand("server", "Show information about the server (same as /info guild)")]
    public async Task serverInfo() => await GuildInfo();

    [SlashCommand("channel", "Show information about a specific text channel")]
    public async Task channelInfo([Summary(description: "Show information of specified channel")] ITextChannel? channel = null)
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
}