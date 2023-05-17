using MacroBot.Logger;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace MacroBot.StartupConfig;

public static class LoggingConfiguration
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
    {
<<<<<<< HEAD:MacroBotApp/Startup/LoggingConfiguration.cs
        webApplicationBuilder.Host.UseSerilog((_, services, configuration) =>
=======
        return hostBuilder.UseSerilog((_, services, configuration) =>
>>>>>>> 9eb4fad4dcae341cb92e06706d6e23ec748ddf0b:MacroBotApp/StartupConfig/LoggingConfiguration.cs
            configuration
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Debug)
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.DiscordSink(services));
    }
}