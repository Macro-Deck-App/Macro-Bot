using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MacroBot.Config;
using MacroBot.DataAccess;
using MacroBot.DataAccess.AutoMapper;
using MacroBot.DataAccess.Repositories;
using MacroBot.DataAccess.RepositoryInterfaces;
using MacroBot.Discord.Modules.Tagging;
using MacroBot.Extensions;
using MacroBot.ServiceInterfaces;
using MacroBot.Services;

namespace MacroBot.Startup;

public static class DependencyInjection
{
    public static async Task ConfigureServicesAsync(this IServiceCollection services)
    {
        // ReSharper disable once HeapView.ClosureAllocation
        var botConfig = await BotConfig.LoadAsync(Paths.BotConfigPath);
        var commandsConfig = await CommandsConfig.LoadAsync(Paths.CommandsConfigPath);
        var statusCheckConfig = await StatusCheckConfig.LoadAsync(Paths.StatusCheckConfigPath);
        var webhooksConfig = await WebhooksConfig.LoadAsync(Paths.WebhooksPath);
		
        DiscordSocketConfig discordSocketConfig = new()
        {
            AlwaysDownloadUsers = true,
            MaxWaitBetweenGuildAvailablesBeforeReady = (int)new TimeSpan(0, 0, 15).TotalMilliseconds,
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.All,
        };

        InteractionServiceConfig interactionServiceConfig = new()
        {
            AutoServiceScopes = true,
            DefaultRunMode = RunMode.Async
        };
        
        services.AddDbContext<MacroBotContext>();
        services.AddAutoMapper(typeof(TagMapping));
        services.AddTransient<ITagRepository, TagRepository>();
        services.AddTransient<TaggingUtils>();
        services.AddSingleton(botConfig);
        services.AddSingleton(commandsConfig);
        services.AddSingleton(discordSocketConfig);
        services.AddSingleton(statusCheckConfig);
        services.AddSingleton(webhooksConfig);
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), interactionServiceConfig));
        services.AddSingleton<CommandHandler>();
        services.AddInjectableHostedService<IDiscordService, DiscordService>();
        services.AddInjectableHostedService<IStatusCheckService, StatusCheckService>();
        services.AddInjectableHostedService<ITimerService, TimerService>();
        services.AddHttpClient();
        services.AddSwagger();
        services.AddControllers();
    }
}