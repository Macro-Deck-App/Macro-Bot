using MacroBot.DataAccess;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MacroBot.Extensions;

public static class AppExtensions
{
	public static async Task MigrateDatabaseAsync(this IHost app)
	{
		using var scope = app.Services.CreateScope();
		var macroBotContext = scope.ServiceProvider.GetRequiredService<MacroBotContext>();
		Log.Information("Starting database migration...");
		await macroBotContext.Database.MigrateAsync();
		Log.Information("Database migration finished");
	}
}