namespace MacroBot;

public class Constants
{
    public const string MainDirectory = "Config";
    public static string BotConfigPath = Path.Combine(MainDirectory, "BotConfig.json");
    public static string CommandsConfigPath = Path.Combine(MainDirectory, "Commands.json");
    public static string DatabasePath = Path.Combine(MainDirectory, "Database.db");
}