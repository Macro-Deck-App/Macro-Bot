namespace MacroBot.Commands;

public class StatusCommands {
    public async static Task<HttpResponseMessage?> CheckStatus(string url, Action<int> action)
    {
        using var client = new HttpClient();
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

    public static void AsIf<T>(object? value, Action<T> action) where T : class
    {
        var t = value as T;
        if (t != null)
        {
            action(t);
        }
    }
}