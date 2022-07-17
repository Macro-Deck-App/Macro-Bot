using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Develeon64.MacroBot.Commands
{
    public class TestCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("test", "Test Command")]
        public async Task Test(string testInput = null)
        {
            await RespondAsync(testInput);
        }


    }
}
