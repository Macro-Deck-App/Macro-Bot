using Develeon64.MacroBot.Utils;
using Develeon64.MacroBot.Models;
using Develeon64.MacroBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using Develeon64.MacroBot.Commands.Tagging;
using Develeon64.MacroBot.Commands.Plugins;

namespace Develeon64.MacroBot.Commands
{
    public class ModerationUtils
    {
        public static async Task TimeoutAsync(IUser user, IGuild guild, TimeSpan time) {
            var usertoTimeout = await guild.GetUserAsync(user.Id);
            if (usertoTimeout is null) {
                throw new InvalidOperationException("User specified isn't on the guild specified");
            }
            try {
                await usertoTimeout.SetTimeOutAsync(time);
            }
            catch (Exception e) {
                throw new Exception("Cannot timeout the user specified");
            }
        }

        public static async Task BanAsync(IUser user, IGuild guild, string reason, int prune = 0) {
            if (prune < 0 || prune > 0) {
                throw new ArgumentException("Prune isn't on the accepted mark.");
            }
            var usertoTimeout = await guild.GetUserAsync(user.Id);
            if (usertoTimeout is null) {
                throw new InvalidOperationException("User specified isn't on the guild specified");
            }
            try {
                await usertoTimeout.BanAsync(prune, reason);
            }
            catch (Exception e) {
                throw new Exception("Cannot ban the user specified");
            }
        }
    }
}