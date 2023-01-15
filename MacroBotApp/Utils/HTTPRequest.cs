using System.Net;

namespace MacroBot.Utils;

public static class HttpRequest {
    public static async Task<string> GetAsync(string uri)
    {
        var request = (HttpWebRequest)WebRequest.Create(uri);
        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        using var response = (HttpWebResponse)await request.GetResponseAsync();
        await using var stream = response.GetResponseStream();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}