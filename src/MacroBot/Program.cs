using MacroBot.Core;
using MacroBot.Core.Extensions;
using MacroBot.Core.Runtime;
using MacroBot.StartupConfig;
using Serilog;

namespace MacroBot;

public static class Program
{
	public static async Task Main(string[] args)
	{
		AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
		Paths.EnsureDirectoriesCreated();
		
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

		await app.MigrateDatabaseAsync();
		await app.RunAsync();
	}

	private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Log.Fatal((Exception)e.ExceptionObject, "Unhandled exception");
	}
}