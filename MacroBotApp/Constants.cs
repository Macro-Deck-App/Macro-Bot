namespace MacroBot;

public class Constants
{
    public static string MainDirectory
    {
        get
        {
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            return environment == "DEVELOPMENT" ? "/etc/macro bot dev" : "/etc/macro bot";
        }
    }

    public static string BotConfigPath = Path.Combine(MainDirectory, "BotConfig.json");
    public static string CommandsConfigPath = Path.Combine(MainDirectory, "Commands.json");
    public static string StatusCheckConfigPath = Path.Combine(MainDirectory, "StatusCheck.json");
    public static string DatabasePath = Path.Combine(MainDirectory, "Database.db");

    public const string StatusUpdateMessageTitle = "Macro Deck Service Status";
}