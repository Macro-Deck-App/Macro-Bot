using MacroBot.Core;
using MacroBot.Core.Extensions;
using MacroBot.StartupConfig;
using Serilog;

namespace MacroBot;

public static class Program
{
	public static async Task Main(string[] args)
	{
		AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
		Paths.EnsureDirectoriesCreated();
		
		var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
		var port = 80;
		if (string.IsNullOrWhiteSpace(environment))
		{
			port = 9000;
		}
		
		var app = Host.CreateDefaultBuilder(args)
			.ConfigureSerilog()
			.ConfigureWebHostDefaults(hostBuilder =>
			{
				hostBuilder.UseStartup<Startup>();
				hostBuilder.ConfigureKestrel(options =>
				{
					options.ListenAnyIP(port);
					options.AllowSynchronousIO = true;
				});
			}).Build();

		await app.MigrateDatabaseAsync();
		await app.RunAsync();
	}

	private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Log.Fatal((Exception)e.ExceptionObject, "Unhandled exception");
	}
}