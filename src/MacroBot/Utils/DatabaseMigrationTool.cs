using EvolveDb;
using MacroBot.Core.Config;
using Npgsql;
using Serilog;

namespace MacroBot.Utils;

public class DatabaseMigrationTool
{
    public static void MigrateDatabase()
    {
        try
        {
            var connection = new NpgsqlConnection(MacroBotConfig.DatabaseConnectionString);
            var evolve = new Evolve(connection, Log.Information)
            {
                Locations = new[] { "Migrations" },
                IsEraseDisabled = true,
                Schemas = new []{ "evolve" }
            };

            evolve.Migrate();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database migration failed");
            throw;
        }
    }
}