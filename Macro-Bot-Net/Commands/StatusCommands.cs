using Discord.Interactions;
using Octokit;
using Discord.WebSocket;
using Develeon64.MacroBot.Utils;
using Discord;
using Newtonsoft.Json;
using System.Net;

namespace Develeon64.MacroBot.Commands {
    public class StatusCommands {
        public async static Task<HttpResponseMessage?> CheckStatus(string url, Action<int> action) {
            using (var client = new HttpClient())
            {
                try {
                    var response = await client.GetAsync(url);
                    action((int)response.StatusCode);
                    if(response.IsSuccessStatusCode) //LINQ
                    {
                        return response;
                    } else {
                        return null;
                    }
                } catch (Exception) {
                    return null;
                }
            }
        }

        public static void AsIf<T>(object? value, Action<T> action) where T : class
        {
            T? t = value as T;
            if (t != null)
            {
                action(t);
            }
        }
    }
}