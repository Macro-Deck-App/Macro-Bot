using Develeon64.MacroBot.Utils;
using Develeon64.MacroBot.Models;
using Develeon64.MacroBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;

namespace Develeon64.MacroBot.Commands
{
    [Group("info", "Show information about the bot, server, channel, or user")]
    public class InfoCommands : InteractionModuleBase<SocketInteractionContext>
    {

        [SlashCommand("bot", "Show information about the bot")]
        public async Task bot()
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = Context.Client.CurrentUser.Username,
                Description = "Here you can find the information of this Discord Bot."
            };
            embed.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());
            embed.AddField("Description", "This is the official Discord Bot for the Macro Deck discord server by <@!300244123569487873>.");
            embed.AddField("Usage", "This bot functions using Discord's Slash Command feature, type `/` in chat in order to view a list of valid commands. Also, this bot is also used on Macro Deck Discord Server's Ticket Support system.");
            embed.AddField("Version", $"`{Assembly.GetExecutingAssembly().GetName().Version}`", true);
            embed.AddField("API Latency", $"`{Context.Client.Latency} ms`", true);
            embed.AddField("Written by", "<@!298215920709664768>\n<@!367398650076463118>\n<@!908002848967626842>", true);
            embed.WithFooter(Context.Guild.Name + " | " + Context.Guild.Id, Context.Guild.IconUrl);

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("user", "Show information about a user")]
        public async Task userInfo([Summary(description: "Shows information about the user")] IUser user)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = user.Username,
                Description = "Here you can find the information of " + $"<@!{user.Id}>" + "."
            };
            embed.WithThumbnailUrl(Context.Client.GetUser(user.Id).GetAvatarUrl());
            embed.AddField("Status", Context.Client.GetUser(user.Id).Status, true);
            embed.AddField("Roles", Context.Guild.GetUser(user.Id).Roles.Count - 1, true);
            embed.AddField("Creation Date", $"<t:" + Context.Client.GetUser(user.Id).CreatedAt.ToUnixTimeSeconds() + ">");
            embed.AddField("Join Date", $"<t:" + Context.Guild.GetUser(user.Id).JoinedAt.Value.ToUnixTimeSeconds() + ">");
            embed.WithFooter("ID: " + Context.Client.GetUser(user.Id).Id);

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("guild", "Show information about the guild (same as /info server)")]
        public async Task guildInfo()
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = Context.Guild.Name,
                Description = "Here you can find the information of this server."
            };
            embed.WithThumbnailUrl(Context.Guild.IconUrl);
            embed.AddField("Owner", "<@" + Context.Guild.OwnerId + ">");
            embed.AddField("Members", Context.Guild.Users.Count, true);
            embed.AddField("Text Channels", Context.Guild.TextChannels.Count, true);
            embed.AddField("Voice Channels", Context.Guild.VoiceChannels.Count, true);
            embed.WithFooter($"ID: {Context.Guild.Id}");

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("server", "Show information about the server (same as /info guild)")]
        public async Task serverInfo() => await guildInfo();

        [SlashCommand("channel", "Show information about a specific channel")]
        public async Task channelInfo()
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = "#" + Context.Channel.Name,
                Description = $"Here you can find the information of <#{Context.Channel.Id}>."
            };
            embed.AddField("Created", $"<t:{Context.Channel.CreatedAt.ToUnixTimeSeconds()}>", true);
            embed.AddField("Type", Context.Channel.GetChannelType(), true);
            embed.WithFooter($"ID: {Context.Channel.Id}");

            await RespondAsync(embed: embed.Build());
        }
    }
}
