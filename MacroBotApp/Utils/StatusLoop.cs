using Discord.WebSocket;
using System.Web;
using Discord;
using Newtonsoft.Json;
using MacroBot.Commands;
using MacroBot.Services;

namespace MacroBot.Utils;

public class POST {
    public string? version {get; set;}
    public string? backgroundColor {get; set;}
    public int width {get; set;}
    public int height {get; set;}
    public float devicePixelRatio {get; set;}
    public string? format {get; set;}
    public Chart? chart {get; set;}
}

public class Chart {
    public string? type {get; set;}
    public Data? data {get; set;}
    public Options2? options {get; set;}
}

public class Options2 {
    public Legend? legend {get; set;}
    public Scales? scales {get; set;}
}

public class Scales {
    public YAxes? yAxes {get; set;}
    public XAxes? xAxes {get; set;}
}

public class YAxes {
    public Labels? ticks {get; set;}
}

public class XAxes {
    public Labels? ticks {get; set;}
}
    
public class Legend {
    public Labels? labels {get; set;}
}

public class Labels {
    public int fontSize {get; set;}
    public string? fontColor {get; set;}
}

public class Data {
    public string[]? labels {get; set;}
    public List<Dataset>? datasets {get; set;}
    public Options? options {get; set;}
}

public class Dataset {
    public string? label {get; set;}
    public string? backgroundColor {get; set;}
    public int? borderWidth {get; set;}
    public int[]? data {get; set;}
}

public class Options {
    public Title? title {get; set;}
}

public class Title {
    public bool? display {get; set;}
    public string? text {get; set;}
}

public class StatusLoop {
    private static readonly HttpClient httpclient = new HttpClient();
    public static async Task<List<List<string>>> StatusLoop1(DiscordSocketClient client, ulong messageid, List<List<string>> strings) {
        StatusLoop statusLoop = new();
        var program = new Program();
        var channel = (client.GetGuild(ConfigManager.GlobalConfig.TestGuildId).GetChannel(ConfigManager.GlobalConfig.Channels.UpdateChannelId) as ITextChannel);

        var website = 0;
        var webclie = 0;
        var extstor = 0;
        var exstapi = 0;
        var updtapi = 0;
        var webs = 0;
        var webc = 0;
        var exts = 0;
        var exst = 0;
        var updt = 0;

        StatusCommands.AsIf<HttpResponseMessage>(await StatusCommands.CheckStatus("http://macro-deck.app/", code => webs = code), response => {
            website = 1;
        });
        StatusCommands.AsIf<HttpResponseMessage>(await StatusCommands.CheckStatus("http://web.macrodeck.org/", code => webc = code), response => {
            webclie = 1;
        });
        StatusCommands.AsIf<HttpResponseMessage>(await StatusCommands.CheckStatus("http://extensionstore.macro-deck.app/", code => exts = code), response => {
            extstor = 1;
        });
        StatusCommands.AsIf<HttpResponseMessage>(await StatusCommands.CheckStatus("https://extensionstore.api.macro-deck.app/Extensions", code => exst = code), response => {
            exstapi = 1;
        });
        StatusCommands.AsIf<HttpResponseMessage>(await StatusCommands.CheckStatus("https://macrodeck.org/files/versions?latest", code => updt = code), response => { // http://update.api.macro-deck.app/
            updtapi = 1;
        });

        EmbedBuilder embed = new();
        embed.WithTitle("Macro Deck Service Status");
        embed.WithDescription((website == 1 && webclie == 1 && extstor == 1 && exstapi == 1 && updtapi == 1)? "✅ All services are online. You should have no problems." : "⚠ There is a problem on one or more services. We are working to the issue.");
        embed.WithColor((website == 1 && webclie == 1 && extstor == 1 && exstapi == 1 && updtapi == 1)? Color.Green : Color.Gold);
        var type = (website == 1)? "🟢" : "🔴";
        var typ2 = (webclie == 1)? "🟢" : "🔴";
        var typ3 = (extstor == 1)? "🟢" : "🔴";
        var typ4 = (exstapi == 1)? "🟢" : "🔴";
        var typ5 = (updtapi == 1)? "🟢" : "🔴";
        var typ = (website == 1)? "o" : "x";
        var ty2 = (webclie == 1)? "o" : "x";
        var ty3 = (extstor == 1)? "o" : "x";
        var ty4 = (exstapi == 1)? "o" : "x";
        var ty5 = (updtapi == 1)? "o" : "x";

        strings[0].Insert(0, typ);
        strings[0].RemoveAt(strings[0].Count - 1);
        strings[1].Insert(0, ty2);
        strings[1].RemoveAt(strings[1].Count - 1);
        strings[2].Insert(0, ty3);
        strings[2].RemoveAt(strings[2].Count - 1);
        strings[3].Insert(0, ty4);
        strings[3].RemoveAt(strings[3].Count - 1);
        strings[4].Insert(0, ty5);
        strings[4].RemoveAt(strings[4].Count - 1);

        Chart graph = new();
        Dataset dataset = new();
        Dataset dataset2 = new();
        Data data = new();
        data.labels = new string[] {"Macro Deck Website", "Macro Deck Web Client", "Macro Deck Extension Store", "Macro Deck Extension Store API", "Macro Deck Update API"};
        List<Dataset> datasets = new();
        graph.type = "bar";
        graph.data = data;
        dataset2.label = "Downtime";
        dataset2.borderWidth = 2;
        dataset2.backgroundColor = "rgb(235, 51, 51)";
        dataset.label = "Uptime";
        dataset.borderWidth = 2;
        dataset.backgroundColor = "rgb(22, 138, 32)";
        var st0 = strings[0].Where(str => str.Contains("o")).Count();
        var st1 = strings[1].Where(str => str.Contains("o")).Count();
        var st2 = strings[2].Where(str => str.Contains("o")).Count();
        var st3 = strings[3].Where(str => str.Contains("o")).Count();
        var st4 = strings[4].Where(str => str.Contains("o")).Count();
        var std0 = strings[0].Where(str => str.Contains("x")).Count();
        var std1 = strings[1].Where(str => str.Contains("x")).Count();
        var std2 = strings[2].Where(str => str.Contains("x")).Count();
        var std3 = strings[3].Where(str => str.Contains("x")).Count();
        var std4 = strings[4].Where(str => str.Contains("x")).Count();
        dataset.data = new int[] { st0, st1, st2, st3, st4 };
        dataset2.data = new int[] { std0, std1, std2, std3, std4 };
        datasets.Add(dataset);
        datasets.Add(dataset2);
        data.datasets = datasets;

        var json = JsonConvert.SerializeObject(graph, Formatting.None);
        var jstr = HttpUtility.UrlEncode(json);

        embed.AddField("Macro Deck website", $"Status: {type} ({((website == 1)? "Online" : "Offline")}, {webs})");
        embed.AddField("Macro Deck Web Client", $"Status: {typ2} ({((webclie == 1)? "Online" : "Offline")}, {webc})");
        embed.AddField("Extension Store", $"Status: {typ3} ({((extstor == 1)? "Online" : "Offline")}, {exts})");
        embed.AddField("Extension Store API", $"Status: {typ4} ({((exstapi == 1)? "Online" : "Offline")}, {exst})");
        embed.AddField("Macro Deck Update API", $"Status: {typ5} ({((updtapi == 1)? "Online" : "Offline")}, {updt})");
        embed.WithImageUrl($"https://quickchart.io/chart?width=500&height=300&backgroundColor=%23b3c6e6&chart={jstr}");
            
        embed.WithFooter("Updated on");
        embed.WithCurrentTimestamp();

        try {
            var msg = await channel!.GetMessageAsync(messageid);
            await channel!.ModifyMessageAsync(messageid, m => {m.Content = "Macro Deck Service Status";});
        } catch (Exception) {
            await channel!.DeleteMessagesAsync(await channel.GetMessagesAsync(10).FlattenAsync());
            Thread.Sleep(1000);

            var msg = await channel!.SendMessageAsync("Macro Deck Service Status");
            program.messageid = msg.Id;
        }
        await channel!.ModifyMessageAsync(program.messageid, m => {m.Content = "Macro Deck Service Status";m.Embed = embed.Build();});

        return strings;
    }
}