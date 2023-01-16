using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;

namespace MacroBot.Services;

public class CommandHandler
{
    private readonly ILogger _logger = Log.ForContext<CommandHandler>();
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            // add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;
            
            _logger.Information("Command handler initialized - {NoModules} loaded - {Modules}",
                _commands.Modules.Count,
                string.Join(",", _commands.Modules.Select(x => x.Name)));
        }
        
        private async Task HandleInteraction (SocketInteraction arg)
        {
            try
            {
                // create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                var ctx = new SocketInteractionContext(_client, arg);
                await _commands.ExecuteCommandAsync(ctx, _services);
                _logger.Information(
                    "{Interaction} executed by {User}", 
                    ctx.Interaction.Type, 
                    ctx.User.Username);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    "Error handling interaction - {Exception}",
                    ex);
                // if a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if(arg.Type == InteractionType.ApplicationCommand)
                {
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
                }
            }
        }
    }