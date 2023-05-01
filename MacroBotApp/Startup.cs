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
using MacroBot.StartupConfig;

namespace MacroBot;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        LoadRegisterConfigs(services);
		
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
        services.AddSingleton(discordSocketConfig);
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
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors("AllowAny");
        app.UseFileServer();
        app.UseWebSockets(new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2)
        });
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        app.ConfigureSwagger();
    }
    
    private static void LoadRegisterConfigs(IServiceCollection services)
    {
        Task.Run(async () =>
        {
            var botConfig = await BotConfig.LoadAsync(Paths.BotConfigPath);
            var commandsConfig = await CommandsConfig.LoadAsync(Paths.CommandsConfigPath);
            var statusCheckConfig = await StatusCheckConfig.LoadAsync(Paths.StatusCheckConfigPath);
            var webhooksConfig = await WebhooksConfig.LoadAsync(Paths.WebhooksPath);
            
            
            services.AddSingleton(statusCheckConfig);
            services.AddSingleton(webhooksConfig);
            services.AddSingleton(botConfig);
            services.AddSingleton(commandsConfig);
        }).Wait();
    }
}