using Develeon64.MacroBot.Models;
using Develeon64.MacroBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Develeon64.MacroBot.Commands.Polls
{
    [Group("poll", "Poll System")]
    public class PollCommands : InteractionModuleBase<SocketInteractionContext>
    {
        public static List<PollChannelAssignable> createAssignables = new();

        [SlashCommand("create", "Create a new poll")]
        public async Task Create([Summary(description: "The Answer options for this poll")] PollOption option ,[Summary("channel","the channel to post the poll in")] ITextChannel? channel = null)
        {
            // Handle Permissions
            SocketGuildUser guildUser = Context.Guild.GetUser(Context.User.Id);
            ulong[] requiredPermissions = ConfigManager.CommandsConfig.Polls.PermissionManagePolls;
            if (!PollUtils.checkPermissions(requiredPermissions, guildUser))
            {

                await RespondAsync(embed: PollUtils.buildPermissionError(requiredPermissions), ephemeral: true);
                return;
            }

            PollChannelAssignable assignable = createAssignables.Find(x => x.user == Context.User.Id);
            if (assignable != null) createAssignables.Remove(assignable);
            createAssignables.Add(new PollChannelAssignable(Context.User.Id, option, channel));

            // Reply with Modal
            await RespondWithModalAsync<PollCreateModal>("poll_create_modal");
        }
    }
}
