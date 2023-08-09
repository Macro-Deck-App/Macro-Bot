using Discord;
using Discord.WebSocket;
using MacroBot.Core.Config;
using MacroBot.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace MacroBot.Core.Logger;

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

        var logChannel = _discordSocketClient.GetGuild(MacroBotConfig.GuildId)
            ?.GetChannel(MacroBotConfig.LogChannelId) as ITextChannel;

        var errorChannel = _discordSocketClient.GetGuild(MacroBotConfig.GuildId)
            ?.GetChannel(MacroBotConfig.ErrorLogChannelId) as ITextChannel;

        if (logChannel is null && errorChannel is null)
        {
            return;
        }

        var adminRole = _discordSocketClient.GetGuild(MacroBotConfig.GuildId).Roles?
            .FirstOrDefault(x => x.Id == MacroBotConfig.AdministratorRoleId);

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
        embedBuilder.AddField("Message", StringExtensions.Truncate(message, 1023));

        if (logEvent.Exception is not null)
        {
            embedBuilder.AddField("Exception", StringExtensions.Truncate(logEvent.Exception.Message, 1023));
            embedBuilder.AddField("Stack Trace", StringExtensions.Truncate(logEvent.Exception.StackTrace, 1023));
        }

        var text = adminRole is not null && logEvent.Level > LogEventLevel.Warning ? adminRole.Mention : null;


        if (logEvent.Level >= LogEventLevel.Warning)
        {
            Task.Run(async () => await SendSafeAsync(errorChannel, text, embedBuilder.Build()));   
        }
        else
        {
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