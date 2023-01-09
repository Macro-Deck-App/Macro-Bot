using Discord.Interactions;
using Octokit;
using Discord.WebSocket;
using Develeon64.MacroBot.Utils;
using Discord;
using Develeon64.MacroBot.Commands;
using Develeon64.MacroBot.Services;
using Newtonsoft.Json;
using System.Net;

namespace Develeon64.MacroBot.Utils {
    public class StatusLoop {

        public async Task DelMsgs(int count, ITextChannel channel) {
            await channel.DeleteMessagesAsync(await channel.GetMessagesAsync(count).FlattenAsync());
        }

        public static async Task StatusLoop1(DiscordSocketClient client, ulong messageid) {
            StatusLoop statusLoop = new();
            Program program = new Program();
            var channel = (client.GetGuild(ConfigManager.GlobalConfig.TestGuildId).GetChannel(ConfigManager.GlobalConfig.Channels.UpdateChannelId) as ITextChannel);

            int website = 0;
            int webclie = 0;
            int extstor = 0;
            int exstapi = 0;
            int updtapi = 0;

            StatusCommands.AsIf<HttpResponseMessage>(await StatusCommands.CheckStatus("https://macro-deck.app/"), response => {
                website = 1;
            });
            StatusCommands.AsIf<HttpResponseMessage>(await StatusCommands.CheckStatus("https://web.macro-deck.app/"), response => {
                webclie = 1;
            });
            StatusCommands.AsIf<HttpResponseMessage>(await StatusCommands.CheckStatus("https://extensionstore.macro-deck.app/"), response => {
                extstor = 1;
            });
            StatusCommands.AsIf<HttpResponseMessage>(await StatusCommands.CheckStatus("https://extensionstore.api.macro-deck.app/"), response => {
                exstapi = 1;
            });
            StatusCommands.AsIf<HttpResponseMessage>(await StatusCommands.CheckStatus("https://update.api.macro-deck.app/"), response => {
                updtapi = 1;
            });

            EmbedBuilder embed = new();
            embed.WithTitle("Macro Deck Service Status");
            embed.WithDescription((website == 1 && webclie == 1 && extstor == 1 && exstapi == 1 && updtapi == 1)? "✅ All services are online. You should have no problems." : "⚠ There is a problem on one or more services. We are working to the issue.");
            string type = (website == 1)? "🟢" : "🔴";
            string typ2 = (webclie == 1)? "🟢" : "🔴";
            string typ3 = (extstor == 1)? "🟢" : "🔴";
            string typ4 = (exstapi == 1)? "🟢" : "🔴";
            string typ5 = (updtapi == 1)? "🟢" : "🔴";
            embed.AddField("Service", $"{type} Macro Deck website\r\n{typ2} Web Client\r\n{typ3} Extension Store\r\n{typ4} Extension Store API\r\n{typ5} MD Update API", true);
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
        }
    }
}