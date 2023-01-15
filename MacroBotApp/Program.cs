using Discord;
using Newtonsoft.Json;
using Discord.Net;
using Discord.WebSocket;
using Discord.Interactions;
using System.Timers;
using MacroBot.Commands;
using MacroBot.Config;
using MacroBot.Extensions;
using MacroBot.Logging;
using MacroBot.Models;
using MacroBot.ServiceInterfaces;
using MacroBot.Services;
using MacroBot.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MacroBot;

public class Program {
	
	private const string BotConfigPath = "Config/BotConfig.json";
	private const string CommandsConfigPath = "Config/Commands.json";

	public static async Task Main(string[] args)
	{
		Log.Logger = new LoggerConfiguration()
			.WriteTo.Console()
			.CreateLogger();
		
		// ReSharper disable once HeapView.ClosureAllocation
		var botConfig = await BotConfig.LoadAsync(BotConfigPath);
		var commandsConfig = await CommandsConfig.LoadAsync(CommandsConfigPath);
		
		DiscordSocketConfig discordSocketConfig = new() {
			AlwaysDownloadUsers = true,
			MaxWaitBetweenGuildAvailablesBeforeReady = (int)new TimeSpan(0, 0, 15).TotalMilliseconds,
			MessageCacheSize = 100,
			GatewayIntents = GatewayIntents.All,
		};
		
		var builder = Host.CreateDefaultBuilder(args)
			.UseSerilog()
			.ConfigureServices(services =>
			{
				services.AddSingleton(botConfig);
				services.AddSingleton(commandsConfig);
				services.AddSingleton(discordSocketConfig);
				services.AddSingleton<DiscordSocketClient>();
				//services.AddInjectableHostedService<IDiscordService, DiscordService>();
			});

		var app = builder.Build();

		await app.RunAsync();
	}

	//public static Task Main (string[] args) => new Program().MainAsync(args);

	public async Task MainAsync (string[] args) {
		Directory.CreateDirectory("DB");
		await DatabaseManager.Initialize("DB/Database.db3");

		

		await Task.Delay(-1);
	}
}