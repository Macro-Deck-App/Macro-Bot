using Discord.WebSocket;
using Serilog;

namespace MacroBot.Discord;

public class DiscordMessageFilter
{
    
    /// <summary>
    /// Returns true if the message contains at least one image
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static bool FilterForImageChannels(SocketMessage message)
    {
        if (message.Attachments.Count == 0)
        {
            return false;
        }
        Log.Information(string.Join(",", message.Attachments.Select(x => x.ContentType)));
        Log.Information(string.Join(",", message.Attachments.Select(x => x.ContentType.ToLower().Contains("image"))));
        return message.Attachments.All(x => x.ContentType.ToLower().Contains("image"));
    }
}