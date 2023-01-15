using System.Text.Json;
using Serilog;

namespace MacroBot.Config;

public class LoadableConfig<T> where T : new()
{
    public static async Task<T> LoadAsync(string path)
    {
        var logger = Log.ForContext<T>();

        ArgumentException.ThrowIfNullOrEmpty(path);
        if (!File.Exists(path))
        {
            await WriteDefaultAsync(path);
        }

        try
        {
            var content = await File.ReadAllTextAsync(path);
            var config = JsonSerializer.Deserialize<T>(content);

            if (config is not null)
            {
                logger.Information("{Type} loaded", typeof(T));
                return config;
            }
        }
        catch (Exception ex)
        {
            logger.Fatal(
                "Unable to load {Type}", typeof(T));
        }

        return new T();
    }

    private static async Task WriteDefaultAsync(string path)
    {
        var logger = Log.ForContext<T>();
        if (File.Exists(path))
        {
            return;
        }

        var config = new T();
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var configString = JsonSerializer.Serialize(config, options);
        try
        {
            await File.WriteAllTextAsync(path, configString);
        }
        catch (Exception ex)
        {
            logger.Fatal(
                "Cannot write {Type}: {Exception}",
                typeof(T),
                ex);
        }
    }
}