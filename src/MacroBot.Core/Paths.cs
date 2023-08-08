namespace MacroBot.Core;

public static class Paths
{
    private static string ConfigDirectory => "Config";
    
    public static readonly string WebhooksPath = Path.Combine(ConfigDirectory, "Webhooks.json");
    public static readonly string BotConfigPath = Path.Combine(ConfigDirectory, "BotConfig.json");
    public static readonly string CommandsConfigPath = Path.Combine(ConfigDirectory, "Commands.json");
    public static readonly string StatusCheckConfigPath = Path.Combine(ConfigDirectory, "StatusCheck.json");
    public static readonly string DatabasePath = Path.Combine(ConfigDirectory, "Database.db");
    public static readonly string KoFiConfigPath = Path.Combine(ConfigDirectory, "KoFi.json");

    public static void EnsureDirectoriesCreated()
    {
        CheckAndCreateDirectory(ConfigDirectory);
        static void CheckAndCreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return;
            }

            Directory.CreateDirectory(path);
        }
    }
    
}