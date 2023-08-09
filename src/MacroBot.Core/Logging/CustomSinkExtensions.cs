using Serilog;
using Serilog.Configuration;

namespace MacroBot.Core.Logging;

public static class CustomSinkExtensions
{
    public static LoggerConfiguration DiscordSink(
        this LoggerSinkConfiguration loggerConfiguration,
        IServiceProvider serviceProvider)
    {
        return loggerConfiguration.Sink(new DiscordSink(serviceProvider));
    }
}