using System.Net.Http.Json;
using MacroBot.Core.Models.Extensions;

namespace MacroBot.Core.Extensions;

public static class PluginExtensions
{
    public static async Task<Extension?> GetExtensionAsync(this HttpClient httpClient, string id) {
        return await HttpClientJsonExtensions.GetFromJsonAsync<Extension>(httpClient, $"https://extensionstore.api.macro-deck.app/rest/v2/extensions/{id}");
    }
}