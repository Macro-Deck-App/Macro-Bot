namespace MacroBot;

public static class Paths
{
<<<<<<< HEAD
    public static string WebhooksPath = Path.Combine(MainDirectory, "Webhooks.json");
    public static string BotConfigPath = Path.Combine(MainDirectory, "BotConfig.json");
    public static string CommandsConfigPath = Path.Combine(MainDirectory, "Commands.json");
    public static string StatusCheckConfigPath = Path.Combine(MainDirectory, "StatusCheck.json");
    public static string ExtensionDetectionConfigPath = Path.Combine(MainDirectory, "ExtensionDetection.json");
    public static string DatabasePath = Path.Combine(MainDirectory, "Database.db");

    public static string MainDirectory
=======
    private static string MainDirectory
>>>>>>> 9eb4fad4dcae341cb92e06706d6e23ec748ddf0b
    {
        get
        {
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(environment)) return "__testConfigs__";
            return environment == "DEVELOPMENT" ? "/etc/macro bot dev" : "/etc/macro bot";
        }
    }

<<<<<<< HEAD
=======
    public static readonly string WebhooksPath = Path.Combine(MainDirectory, "Webhooks.json");
    public static readonly string BotConfigPath = Path.Combine(MainDirectory, "BotConfig.json");
    public static readonly string CommandsConfigPath = Path.Combine(MainDirectory, "Commands.json");
    public static readonly string StatusCheckConfigPath = Path.Combine(MainDirectory, "StatusCheck.json");
    public static readonly string DatabasePath = Path.Combine(MainDirectory, "Database.db");
    public static readonly string KoFiConfigPath = Path.Combine(MainDirectory, "KoFi.json");

>>>>>>> 9eb4fad4dcae341cb92e06706d6e23ec748ddf0b
    public static void EnsureDirectoriesCreated()
    {
        CheckAndCreateDirectory(MainDirectory);

        static void CheckAndCreateDirectory(string path)
        {
            if (Directory.Exists(path)) return;

            Directory.CreateDirectory(path);
        }
    }
}