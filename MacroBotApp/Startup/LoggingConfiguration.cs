using MacroBot.Logger;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace MacroBot.Startup;

public static class LoggingConfiguration
{
    public static void ConfigureSerilog(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Host.UseSerilog((_, services, configuration) => 
            configuration
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.DiscordSink(services));
    }
}