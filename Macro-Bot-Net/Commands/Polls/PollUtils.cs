using Develeon64.MacroBot.Models;
using Develeon64.MacroBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Develeon64.MacroBot.Commands.Polls
{
    public class PollUtils
    {
        public static Boolean checkPermissions(ulong[] permissions, SocketGuildUser user)
        {
            List<SocketRole> userRoles = user.Roles.ToList<SocketRole>();

            foreach (ulong permission in permissions)
            {
                if (userRoles.Find(x => x.Id.ToString() == permission.ToString()) != null)
                {
                    return true;
                }
            }
            return false;
        }

        public static Embed buildPermissionError(ulong[] permissions)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Title = "Invalid Permissions",
                Description = "You do not meet the permission requirements to run this command!"
            };
            String permissionList = "";
            foreach (ulong permissionID in permissions) permissionList += $"\n<@&{permissionID.ToString()}>";
            embedBuilder.AddField("Required Roles", $"At least one of those roles is required:{permissionList}");
            embedBuilder.WithColor(new Color(255, 50, 50));

            return embedBuilder.Build();
        }

        public static String getTagInfoError()
        {
            return "An internal error occured while getting Tag Name information, please try again.";
        }
    }

    public class PollChannelAssignable
    {
        public ulong user;
        public ITextChannel channel;
        public PollOption option;

        public PollChannelAssignable(ulong user, PollOption option, ITextChannel channel)
        {
            this.user = user;
            this.option = option;
            this.channel = channel;
        }
    }

    public enum PollOption
    {
        YesNo,
        OneTwo,
        OneTwoThree,
        OneTwoThreeFour,
    }

    // Enum below is used for database interaction
    public enum PollVoteOption
    {
        Votes1,
        Votes2,
        Votes3,
        Votes4,
    }
}
