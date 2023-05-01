namespace MacroBot;

public static class Paths
{
    private static string MainDirectory
    {
        get
        {
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(environment))
            {
                return "__testConfigs__";
            }
            return environment == "DEVELOPMENT" ? "/etc/macro bot dev" : "/etc/macro bot";
        }
    }

    public static readonly string WebhooksPath = Path.Combine(MainDirectory, "Webhooks.json");
    public static readonly string BotConfigPath = Path.Combine(MainDirectory, "BotConfig.json");
    public static readonly string CommandsConfigPath = Path.Combine(MainDirectory, "Commands.json");
    public static readonly string StatusCheckConfigPath = Path.Combine(MainDirectory, "StatusCheck.json");
    public static readonly string DatabasePath = Path.Combine(MainDirectory, "Database.db");

    public static void EnsureDirectoriesCreated()
    {
        CheckAndCreateDirectory(MainDirectory);
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