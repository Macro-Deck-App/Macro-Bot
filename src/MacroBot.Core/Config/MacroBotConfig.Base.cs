using System.Net.Http.Json;
using MacroBot.Core.Models.StatusCheck;
using MacroBot.Core.Models.Webhook;
using MacroBot.Core.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Serilog;

namespace MacroBot.Core.Config;

public partial class MacroBotConfig
{
    private static readonly ILogger Logger = Log.ForContext(typeof(MacroBotConfig));

    private static IConfiguration? Configuration { get; set; }

    private const string ConfigPath = "Config/config.json";

    public static int CurrentConfigVersion { get; private set; } = -1;

    public static async ValueTask Initialize()
    {
        Logger.Information("Loading config from {ConfigPath}...", ConfigPath);

        if (MacroBotEnvironment.IsStagingOrProduction)
        {
            Logger.Information(
                "Service was started in staging or production environment. Will download config from ConfigService");
            await UpdateConfig(ConfigPath, CancellationToken.None);
            StartUpdateLoop();
        }

        var configBuilder = new ConfigurationBuilder();
        var config = LoadConfigurationFile(ConfigPath);
        configBuilder.Add(config);

        Configuration = configBuilder.Build();
        Logger.Information("Configuration loaded");
    }

    private static JsonConfigurationSource LoadConfigurationFile(string configPath)
    {
        return new JsonConfigurationSource
        {
            Path = configPath,
            ReloadOnChange = true,
            ReloadDelay = 2000,
            Optional = false
        };
    }

    private static string GetString(string key)
    {
        if (Configuration == null)
        {
            throw new InvalidOperationException("Configuration is not loaded");
        }

        var value = Configuration.GetValue<string>(key);
        if (value != null)
        {
            return value;
        }

        Logger.Fatal("Cannot find value in config for {Key}", key);
        return string.Empty;
    }

    private static StatusCheckItem[] GetStatusCheckItemArray(string key)
    {
        if (Configuration == null)
        {
            throw new InvalidOperationException("Configuration is not loaded");
        }

        var value = Configuration.GetSection(key).Get<StatusCheckItem[]>();
        if (value != null)
        {
            return value;
        }

        Logger.Fatal("Cannot find value in config for {Key}", key);
        return Array.Empty<StatusCheckItem>();
    }

    private static WebhookItem[] GetWebhookItemArray(string key)
    {
        if (Configuration == null)
        {
            throw new InvalidOperationException("Configuration is not loaded");
        }

        var value = Configuration.GetSection(key).Get<WebhookItem[]>();
        if (value != null)
        {
            return value;
        }

        Logger.Fatal("Cannot find value in config for {Key}", key);
        return Array.Empty<WebhookItem>();
    }

    private static ulong GetUlong(string key)
    {
        if (Configuration == null)
        {
            throw new InvalidOperationException("Configuration is not loaded");
        }

        var value = Configuration.GetValue<ulong?>(key);
        if (value.HasValue)
        {
            return value.Value;
        }

        Logger.Fatal("Cannot find value in config for {Key}", key);
        return 0;
    }


    private static ulong[] GetUlongArray(string key)
    {
        if (Configuration == null)
        {
            throw new InvalidOperationException("Configuration is not loaded");
        }

        var value = Configuration.GetSection(key).Get<ulong[]>();
        if (value != null)
        {
            return value;
        }

        Logger.Fatal("Cannot find value in config for {Key}", key);
        return Array.Empty<ulong>();
    }

    private static void StartUpdateLoop()
    {
        Task.Run(async () => await UpdateLoop(CancellationToken.None));
    }

    private static async Task UpdateLoop(CancellationToken cancellationToken)
    {
        do
        {
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            var latestConfigVersion = await GetLatestConfigVersion();
            if (latestConfigVersion == CurrentConfigVersion)
            {
                continue;
            }
            
            Logger.Information(
                "Updating config {CurrentConfigVersion} -> {ConfigVersion}",
                CurrentConfigVersion,
                latestConfigVersion);
            await UpdateConfig(ConfigPath, cancellationToken);
        } while (!cancellationToken.IsCancellationRequested);
    }

    private static async ValueTask UpdateConfig(string configPath, CancellationToken cancellationToken)
    {
        try
        {
            var encodedConfig = await DownloadFromConfigService();
            var configJson = Base64Decode(encodedConfig.ConfigBase64 ?? string.Empty);
            CurrentConfigVersion = encodedConfig.Version;

            await File.WriteAllTextAsync(configPath, configJson, cancellationToken);
            Logger.Information("Config version {Version} downloaded from ConfigService", encodedConfig.Version);
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Could not update config");
        }
    }

    private static async Task<int> GetLatestConfigVersion()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-config-name", new[] { MacroBotEnvironment.ConfigServiceConfigName });
        httpClient.DefaultRequestHeaders.Add("x-config-access-token",
            new[] { MacroBotEnvironment.ConfigServiceAuthToken });

        var response = await httpClient.GetAsync($"{MacroBotEnvironment.ConfigServiceUrl}/config/version");
        var resultString = await response.Content.ReadAsStringAsync();
        return int.TryParse(resultString, out var result) ? result : 0;
    }

    private static async ValueTask<EncodedConfig> DownloadFromConfigService()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("x-config-name", new[] { MacroBotEnvironment.ConfigServiceConfigName });
        httpClient.DefaultRequestHeaders.Add("x-config-access-token",
            new[] { MacroBotEnvironment.ConfigServiceAuthToken });

        return await httpClient.GetFromJsonAsync<EncodedConfig>(
                   $"{MacroBotEnvironment.ConfigServiceUrl}/config/encoded")
               ?? throw new InvalidOperationException("Could not download config");
    }

    private static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
}