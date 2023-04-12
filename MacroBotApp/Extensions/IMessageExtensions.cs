using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;
using System.Text;
using System.Web;
using MacroBot.Models.Translate;
using ILogger = Serilog.ILogger;

namespace MacroBot.Extensions;

public static class IMessageExtensions
{
	/*
    public async static Task<List<Detection>> DetectLang(this IMessage msg, HttpClient httpClient) {
        var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes("macrobottest:iLdbjnrcdH3T357iLQEBUX41LVAhDPKRLqUWd9rKr1"));
		httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);
		Dictionary<string, string> postData = new();
		postData.Add("q", HttpUtility.HtmlEncode(message.Content));
		using (var content = new FormUrlEncodedContent(postData)) {
			content.Headers.Clear();
        	content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
			var response = await httpClient.PostAsJsonAsync<List<Detection>>($"https://translate.api.macro-deck.app/detect?q=", content);


		}
    }
	*/
}