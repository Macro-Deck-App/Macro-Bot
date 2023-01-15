using Discord;
using Discord.WebSocket;
using Serilog;

namespace MacroBot.Extensions;

public static class DiscordSocketClientExtensions
{
    public static void UseSerilog(this DiscordSocketClient client)
    {
        var logger = Log.ForContext<DiscordSocketClient>();
        client.Log += delegate(LogMessage message)
        {
            if (message.Message is null || 
                message.Exception?.Message is null)
            {
                return Task.CompletedTask;
            }

            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    logger.Fatal(
                        message.Message,
                        message.Exception.InnerException);
                    break;
                case LogSeverity.Error:
                    logger.Error(
                        message.Message,
                        message.Exception.InnerException);
                    break;
                case LogSeverity.Warning:
                    logger.Warning(
                        message.Message, 
                        message.Exception.InnerException);
                    break;
                case LogSeverity.Info:
                    logger.Information(
                        message.Message, 
                        message.Exception.InnerException);
                    break;
                case LogSeverity.Verbose:
                    logger.Verbose(
                        message.Message, 
                        message.Exception.InnerException);
                    break;
                case LogSeverity.Debug:
                default:
                    logger.Debug(
                        message.Message, 
                        message.Exception.InnerException);
                    break;
            }

            return Task.CompletedTask;
        };
    }
}