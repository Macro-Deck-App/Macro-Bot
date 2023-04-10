using System.Globalization;
using System.Text;
using System.Text.Json;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using MacroBot.Config;
using MacroBot.Models.Translate;

namespace MacroBot.Discord.Modules.Translate;

[Group("translate", "Translate Commands")]
[UsedImplicitly]
public class TranslateCommands : InteractionModuleBase<SocketInteractionContext>
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly CommandsConfig _commandsConfig;

	public TranslateCommands(IHttpClientFactory httpClientFactory,
		CommandsConfig commandsConfig)
	{
		_httpClientFactory = httpClientFactory;
		_commandsConfig = commandsConfig;
	}
	
    [SlashCommand("word", "Translate a word")]
    public async Task TranslateWord([Summary(description: "The word you want to translate")] string words, [Summary(description:"The language of the word you want to translate. Leave it blank for automatic mode. (default: auto)")]string source = "auto", [Summary(description:"The language you want it to be translated on. Leave it blank if English. (default: en)")] string target = "en")
    {
	    await DeferAsync(ephemeral: true);
	    
        using var httpClient = _httpClientFactory.CreateClient();
        var dict = new Dictionary<string, string>();
        dict.Add("q", words);
        dict.Add("source", source);
        string username = _commandsConfig.Translate.UserName;
        string password = _commandsConfig.Translate.Password;
        string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
        dict.Add("target", target);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://translate.api.macro-deck.app/translate")
        {
        	Content = new FormUrlEncodedContent(dict)
        };
        request.Headers.Add("Authorization", "Basic " + svcCredentials);
        var result = await httpClient.SendAsync(request);
        Console.WriteLine(await result.Content.ReadAsStringAsync());
        var translated = JsonSerializer.Deserialize<Translated>(await result.Content.ReadAsStringAsync());

        await FollowupAsync(embed: new EmbedBuilder()
            {
	            Title = "Macro Bot Translate System",
	            Description = "This is the translated text!"
            }.AddField("Text", translated.TranslatedText).Build(), ephemeral: true);
    }
    
    [SlashCommand("message", "Translate a existing message")]
    public async Task TranslateMessage([Summary(description: "The message you want to translate")] ulong message, [Summary(description:"The language of the message you want to translate. Leave it blank for automatic mode. (default: auto)")]string source = "auto", [Summary(description:"The language you want it to be translated on. Leave it blank if English. (default: en)")] string target = "en")
    {
	    await DeferAsync(ephemeral: true);
	    
        using var httpClient = _httpClientFactory.CreateClient();
        var dict = new Dictionary<string, string>();
        var msg = await Context.Channel.GetMessageAsync(message);
        dict.Add("q", msg.Content);
        dict.Add("source", source);
        string username = _commandsConfig.Translate.UserName;
        string password = _commandsConfig.Translate.Password;
        string svcCredentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
        dict.Add("target", target);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://translate.api.macro-deck.app/translate")
        {
        	Content = new FormUrlEncodedContent(dict)
        };
        request.Headers.Add("Authorization", "Basic " + svcCredentials);
        var result = await httpClient.SendAsync(request);
        Console.WriteLine(await result.Content.ReadAsStringAsync());
        var translated = JsonSerializer.Deserialize<Translated>(await result.Content.ReadAsStringAsync());

        await FollowupAsync(embed: new EmbedBuilder()
            {
	            Title = "Macro Bot Translate System",
	            Description = "This is the translated text!"
            }.AddField("Text", translated.TranslatedText).Build(), ephemeral: true);
    }

    [MessageCommand("Translate to English")]
    public async Task TranslateToEnglish(IMessage msg)
    {
	    await TranslateMessage(msg.Id);
    }
    
    [MessageCommand("Translate to German")]
    public async Task TranslateToGerman(IMessage msg)
    {
	    await TranslateMessage(msg.Id, "auto", "de");
    }
}