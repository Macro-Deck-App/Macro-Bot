using System.Net.Http.Json;
using MacroBot.Core.Config;
using MacroBot.Core.Models.Extensions;

namespace MacroBot.Core.Extensions;

public static class PluginExtensions
{
    public static async Task<Extension?> GetExtensionAsync(this HttpClient httpClient, string id)
    {
        return await httpClient.GetFromJsonAsync<Extension>(
            $"{MacroBotConfig.ExtensionStoreApiUrl}/extensions/{id}");
    }
}