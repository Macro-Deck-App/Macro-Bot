using Serilog;

namespace MacroBot.Core.Runtime;

public class MacroBotEnvironment
{
    private static readonly ILogger Logger = Log.ForContext(typeof(MacroBotEnvironment));
    
    private const string CurrentEnvironmentVariable = "ASPNETCORE_ENVIRONMENT";
    private const string HostingPortVariable = "HOSTING_PORT";
    private const string ConfigServiceConfigNameVariable = "CONFIG_NAME";
    private const string ConfigServiceUrlVariable = "CONFIGSERVICE_URL";
    private const string ConfigServiceAuthTokenVariable = "CONFIGSERVICE_AUTHTOKEN";

    public static bool IsGitHubIntegrationTest => IsCurrentEnvironment("GitHubIntegrationTest");
    public static bool IsLocalDevelopment => IsCurrentEnvironment("LocalDevelopment");
    public static bool IsStaging => IsCurrentEnvironment("Staging");
    public static bool IsProduction => IsCurrentEnvironment("Production");
    public static bool IsStagingOrProduction => IsStaging || IsProduction;
    public static int HostingPort => GetIntFromEnvironmentVariable(HostingPortVariable, 9000);
    public static string ConfigServiceConfigName => GetStringFromEnvironmentVariable(ConfigServiceConfigNameVariable);
    public static string ConfigServiceUrl => GetStringFromEnvironmentVariable(ConfigServiceUrlVariable);
    public static string ConfigServiceAuthToken => GetStringFromEnvironmentVariable(ConfigServiceAuthTokenVariable);

    private static bool IsCurrentEnvironment(string environmentToCompare)
    {
        return GetStringFromEnvironmentVariable(CurrentEnvironmentVariable)
            .Equals(environmentToCompare, StringComparison.InvariantCultureIgnoreCase);
    }
    
    private static string GetStringFromEnvironmentVariable(string environmentVariable, string? fallback = null)
    {
        var result = Environment.GetEnvironmentVariable(environmentVariable);
        if (string.IsNullOrWhiteSpace(result))
        {
            Logger.Fatal("Environment variable {Variable} was empty", environmentVariable);
        }

        return result ?? fallback ?? string.Empty;
    }

    private static int GetIntFromEnvironmentVariable(string environmentVariable, int? fallback = null)
    {
        var variableValue = GetStringFromEnvironmentVariable(environmentVariable);
        if (int.TryParse(variableValue, out var result))
        {
            return result;
        }
        
        Logger.Fatal("Cannot parse environment variable {Variable} to integer", environmentVariable);

        return fallback ?? -1;
    }
}