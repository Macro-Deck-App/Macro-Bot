using Develeon64.MacroBot.Utils;
using Discord.Interactions;
using System.Reflection;

namespace Develeon64.MacroBot.Commands
{
    public class InfoCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("info","Show information about the bot")]
        public async Task Info()
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = Context.Client.CurrentUser.Username,
                Description = "Here you can find some basic information about this Discord Bot."
            };
            embed.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());
            embed.AddField("Usage", "This bot functions using Discord's Slash Command feature, type `/` in chat in order to view a list of valid commands.");
            embed.AddField("Version",$"`{Assembly.GetExecutingAssembly().GetName().Version}`",true);
            embed.AddField("API Latency", $"`{Context.Client.Latency} ms`", true);
            embed.AddField("Written by", "<@298215920709664768>\n<@367398650076463118>",true);
            embed.WithFooter(Context.Guild.Name, Context.Guild.IconUrl);

            await RespondAsync(embed: embed.Build());
        } 
    }
}
