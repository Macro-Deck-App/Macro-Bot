namespace MacroBot;

public static class Paths
{
    public static string MainDirectory
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

    public static string WebhooksPath = Path.Combine(MainDirectory, "Webhooks.json");
    public static string BotConfigPath = Path.Combine(MainDirectory, "BotConfig.json");
    public static string CommandsConfigPath = Path.Combine(MainDirectory, "Commands.json");
    public static string StatusCheckConfigPath = Path.Combine(MainDirectory, "StatusCheck.json");
    public static string ExtensionDetectionConfigPath = Path.Combine(MainDirectory, "ExtensionDetection.json");
    public static string DatabasePath = Path.Combine(MainDirectory, "Database.db");

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
