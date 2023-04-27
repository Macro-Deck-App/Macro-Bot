using MacroBot.Extensions;
using MacroBot.Startup;
using Serilog;

namespace MacroBot;

public static class Program
{
	public static async Task Main(string[] args)
	{
		AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
		Paths.EnsureDirectoriesCreated();
		
		var builder = WebApplication.CreateBuilder(args);
		await builder.Services.ConfigureServicesAsync();
		builder.ConfigureSerilog();
		var app = builder.Build();
		
		app.ConfigureSwagger();
		app.UseCors(x => x
				.AllowAnyMethod()
				.AllowAnyHeader()
				.SetIsOriginAllowed(_ => true) // allow any origin
				.AllowCredentials())
			.UseCookiePolicy();
		app.MapControllers();
		await app.MigrateDatabaseAsync();
		await app.RunAsync();
	}

	private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Log.Fatal((Exception)e.ExceptionObject, "Unhandled exception");
	}
}