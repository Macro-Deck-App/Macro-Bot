using MacroBot.Core.Logging;
using MacroBot.Core.Runtime;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
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
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
                    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Information)
                    .WriteTo.Logger(lc =>
                        lc.Filter.ByIncludingOnly(Matching.FromSource("MacroBot")).WriteTo.DiscordSink(services))
                    .WriteTo.Logger(lc =>
                        lc.Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Warning).WriteTo.DiscordSink(services));
            }
            else
            {
                configuration
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Verbose)
                    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Verbose);
            }

            configuration
                .WriteTo.Console(theme: AnsiConsoleTheme.Code);
        });
    }
}