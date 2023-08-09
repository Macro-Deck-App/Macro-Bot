using MacroBot.Core.Logging;
using MacroBot.Core.Runtime;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace MacroBot.StartupConfig;

public static class LoggingConfiguration
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((_, services, configuration) =>
        {
            if (MacroBotEnvironment.IsStagingOrProduction)
            {
                configuration
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning);
            }
            else
            {
                configuration
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Verbose)
                    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Verbose);
            }
            configuration
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.DiscordSink(services);
        });
    }
}