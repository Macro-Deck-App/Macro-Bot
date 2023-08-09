using Discord;
using Discord.WebSocket;
using MacroBot.Core.Config;
using MacroBot.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace MacroBot.Core.Logging;

public class DiscordSink : ILogEventSink
{
    private readonly DiscordSocketClient? _discordSocketClient;

    public DiscordSink(IServiceProvider serviceProvider)
    {
        _discordSocketClient = serviceProvider.GetService<DiscordSocketClient>();
    }

    public void Emit(LogEvent logEvent)
    {
        if (_discordSocketClient is null || logEvent.Level < LogEventLevel.Information)
        {
            return;
        }

        var guild = _discordSocketClient.GetGuild(MacroBotConfig.GuildId);
        if (guild is null)
        {
            return;
        }
        
        var embedBuilder = new EmbedBuilder
        {
            Color = logEvent.Level switch
            {
                LogEventLevel.Information => Color.Teal,
                LogEventLevel.Warning => Color.Gold,
                LogEventLevel.Error => Color.Red,
                LogEventLevel.Fatal => Color.DarkRed,
                _ => Color.LightGrey
            }
        };

        embedBuilder.AddField("Time/Date (UTC)", $"{DateTime.UtcNow.ToLongTimeString()} {DateTime.UtcNow.ToShortDateString()}");
        
        embedBuilder.AddField("Level", logEvent.Level.ToString());

        var message = logEvent.RenderMessage();
        embedBuilder.AddField("Message", message.Truncate(1023));

        if (logEvent.Exception is not null)
        {
            embedBuilder.AddField("Exception", logEvent.Exception.Message.Truncate(1023));
            embedBuilder.AddField("Stack Trace", logEvent.Exception.StackTrace.Truncate(1023));
        }

        var adminRole = guild.Roles?.FirstOrDefault(x => x.Id == MacroBotConfig.AdministratorRoleId);
        var text = adminRole is not null && logEvent.Level > LogEventLevel.Warning ? adminRole.Mention : null;


        if (logEvent.Level >= LogEventLevel.Warning)
        {
            if (_discordSocketClient.GetGuild(MacroBotConfig.GuildId)
                    ?.GetChannel(MacroBotConfig.ErrorLogChannelId) is not ITextChannel errorChannel)
            {
                return;
            }
            Task.Run(async () => await SendSafeAsync(errorChannel, text, embedBuilder.Build()));   
        }
        else
        {
            if (_discordSocketClient.GetGuild(MacroBotConfig.GuildId)
                    ?.GetChannel(MacroBotConfig.LogChannelId) is not ITextChannel logChannel)
            {
                return;
            }
            Task.Run(async () => await SendSafeAsync(logChannel, text, embedBuilder.Build()));
        }
    }

    private static async Task SendSafeAsync(ITextChannel? channel, string? text, Embed? embed)
    {
        if (channel is null)
        {
            return;
        }
        try
        {
            await channel.SendMessageAsync(text, embed: embed);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to send log message to channel {ChannelId}", channel.Id);
        }
    }
}