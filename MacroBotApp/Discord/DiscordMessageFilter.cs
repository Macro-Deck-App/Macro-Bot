using Discord.WebSocket;
using MacroBot.Config;
using Serilog;

namespace MacroBot.Discord;

public static class DiscordMessageFilter
{
    /// <summary>
    /// Returns true if the message contains at least one image
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static bool FilterForImageChannels(SocketMessage message)
    {
        return message.Attachments.Count != 0 
               && message.Attachments.All(x => x.ContentType.ToLower().Contains("image") 
                                               || x.ContentType.ToLower().Contains("video"));
    }
}