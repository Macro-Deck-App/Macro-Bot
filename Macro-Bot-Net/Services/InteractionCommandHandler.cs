using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Develeon64.MacroBot.Services
{
    public class InteractionCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;

        public InteractionCommandHandler(DiscordSocketClient client, InteractionService commands)
        {
            _client = client;
            _commands = commands;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);

            // Bind InteractionCreated
            _client.InteractionCreated += HandleInteraction;

            // Handle execution results
            _commands.SlashCommandExecuted += SlashCommandExecuted;
            _commands.ContextCommandExecuted += ContextCommandExecuted;
            _commands.ComponentCommandExecuted += ComponentCommandExecuted;
        }

        private Task ComponentCommandExecuted(ComponentCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            return Task.CompletedTask;
        }

        private Task ContextCommandExecuted(ContextCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            return Task.CompletedTask;
        }

        private Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, Discord.Interactions.IResult arg3)
        {
            return Task.CompletedTask;
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                // Build Command Context and run it
                SocketInteractionContext ctx = new SocketInteractionContext(_client, arg);
                await _commands.ExecuteCommandAsync(ctx, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                // Tell the user something went wrong
                if (arg.Type == InteractionType.ApplicationCommand)
                {
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
                }
            }
        }

        public InteractionService GetInteractionService()
        {
            return _commands;
        }
    }
}
