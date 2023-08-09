using MacroBot.Core.Config;
using MacroBot.Core.Runtime;
using MacroBot.StartupConfig;
using MacroBot.Utils;
using Serilog;

namespace MacroBot;

public static class Program
{
	public static async Task Main(string[] args)
	{
		AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
		AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

		await MacroBotConfig.Initialize();
		
		DatabaseMigrationTool.MigrateDatabase();
		
		var app = Host.CreateDefaultBuilder(args)
			.ConfigureSerilog()
			.ConfigureWebHostDefaults(hostBuilder =>
			{
				hostBuilder.UseStartup<Startup>();
				hostBuilder.ConfigureKestrel(options =>
				{
					options.ListenAnyIP(MacroBotEnvironment.HostingPort);
					options.AllowSynchronousIO = true;
				});
			}).Build();

		await app.RunAsync();
	}

	private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Log.Fatal((Exception)e.ExceptionObject, "Unhandled exception");
	}
}