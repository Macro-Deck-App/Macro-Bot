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
using Serilog;

namespace MacroBot;

public static class Program {
	public static async Task Main(string[] args)
	{
		Log.Logger = new LoggerConfiguration()
			.WriteTo.Console()
			.CreateLogger();
		
		Paths.EnsureDirectoriesCreated();
		
		// ReSharper disable once HeapView.ClosureAllocation
		var botConfig = await BotConfig.LoadAsync(Paths.BotConfigPath);
		var commandsConfig = await CommandsConfig.LoadAsync(Paths.CommandsConfigPath);
		var statusCheckConfig = await StatusCheckConfig.LoadAsync(Paths.StatusCheckConfigPath);
		var webhooksConfig = await WebhooksConfig.LoadAsync(Paths.WebhooksPath);
		
		DiscordSocketConfig discordSocketConfig = new() {
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

		var builder = WebApplication.CreateBuilder(args);
		builder.Services.AddDbContext<MacroBotContext>();
		builder.Services.AddAutoMapper(typeof(TagMapping));
		builder.Services.AddTransient<ITagRepository, TagRepository>();
		builder.Services.AddTransient<TaggingUtils>();
		builder.Services.AddSingleton(botConfig);
		builder.Services.AddSingleton(commandsConfig);
		builder.Services.AddSingleton(discordSocketConfig);
		builder.Services.AddSingleton(statusCheckConfig);
		builder.Services.AddSingleton(webhooksConfig);
		builder.Services.AddSingleton<DiscordSocketClient>();
		builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), interactionServiceConfig));
		builder.Services.AddSingleton<CommandHandler>();
		builder.Services.AddInjectableHostedService<IDiscordService, DiscordService>();
		builder.Services.AddInjectableHostedService<IStatusCheckService, StatusCheckService>();
		builder.Services.AddInjectableHostedService<ITimerService, TimerService>();
		builder.Services.AddHttpClient();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();
		builder.Services.AddControllers();
		builder.Host.UseSerilog();

		var app = builder.Build();
		app.UseSwagger();
		app.UseSwaggerUI(c =>
		{
			c.SwaggerEndpoint("/swagger/v1/swagger.json", "Macro Bot API");
			c.RoutePrefix = "";
		});

		app.UseCors(x => x
			.AllowAnyMethod()
			.AllowAnyHeader()
			.SetIsOriginAllowed(origin => true) // allow any origin
			.AllowCredentials());
		
		app.MapControllers();
		app.UseCookiePolicy();

		await app.MigrateDatabaseAsync();
		await app.RunAsync();
	}
}