using Discord.Interactions;

namespace MacroBot.Modules;

public class EchoModule : InteractionModuleBase
{
    [SlashCommand("echo", "Echo an input")]
    public async Task Echo(string input)
    {
        await RespondAsync(input);
    }
}