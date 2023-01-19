using Discord;
using Discord.WebSocket;
using MacroBot.Config;
using MacroBot.ServiceInterfaces;
using Serilog.Core;
using Serilog.Events;

namespace MacroBot.Logger;

public class DiscordSink : ILogEventSink
{
    private readonly DiscordSocketClient? _discordSocketClient;
    private readonly BotConfig? _botConfig;

    public DiscordSink(IServiceProvider serviceProvider)
    {
        _discordSocketClient = serviceProvider.GetService<DiscordSocketClient>();
        _botConfig = serviceProvider.GetService<BotConfig>();
    }

    public void Emit(LogEvent logEvent)
    {
        if (_discordSocketClient is null || _botConfig is null || logEvent.Level < LogEventLevel.Warning)
        {
            return;
        }
        if (_discordSocketClient.GetGuild(_botConfig.GuildId)
                ?.GetChannel(_botConfig.Channels.LogChannelId) is not ITextChannel channel)
        {
            return;
        }

        var adminRole = _discordSocketClient.GetGuild(_botConfig.GuildId).Roles?
            .FirstOrDefault(x => x.Id == _botConfig.Roles.AdministratorRoleId);

        var embedBuilder = new EmbedBuilder
        {
            Color = logEvent.Level switch
            {
                LogEventLevel.Warning => Color.Gold,
                LogEventLevel.Error => Color.Red,
                LogEventLevel.Fatal => Color.DarkRed,
                _ => Color.Default
            }
        };

        embedBuilder.AddField("Time/Date (UTC)", $"{DateTime.UtcNow.ToLongTimeString()} {DateTime.UtcNow.ToShortDateString()}");
        
        embedBuilder.AddField("Level", logEvent.Level.ToString());

        var message = logEvent.RenderMessage();
        embedBuilder.AddField("Message", message);

        if (logEvent.Exception is not null)
        {
            embedBuilder.AddField("Exception", logEvent.Exception.Message);
            embedBuilder.AddField("Stack Trace", logEvent.Exception.StackTrace);
        }

        var text = adminRole is not null && logEvent.Level > LogEventLevel.Warning ? adminRole.Mention : null;

        Task.Run(() => channel.SendMessageAsync(text, embed: embedBuilder.Build()));
    }
}