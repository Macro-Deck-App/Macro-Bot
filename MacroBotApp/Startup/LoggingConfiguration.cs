using MacroBot.Logger;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace MacroBot.Startup;

public static class LoggingConfiguration
{
    public static void ConfigureSerilog(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Host.UseSerilog((_, services, configuration) => 
            configuration
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Debug) 
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.DiscordSink(services));
    }
}