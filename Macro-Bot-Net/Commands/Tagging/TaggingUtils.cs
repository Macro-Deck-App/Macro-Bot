using Develeon64.MacroBot.Models;
using Develeon64.MacroBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Develeon64.MacroBot.Commands.Tagging
{
    public class TaggingUtils
    {
        public static async Task AutoCompleteTag(SocketInteractionContext Context)
        {
            string userInput = (Context.Interaction as SocketAutocompleteInteraction).Data.Current.Value.ToString();

            List<AutocompleteResult> resultList = new();
            foreach (Tag tag in await DatabaseManager.GetTagsForGuild(Context.Guild.Id))
            {
                resultList.Add(new AutocompleteResult(tag.Name, tag.Name));
            }
            IEnumerable<AutocompleteResult> results = resultList.AsEnumerable().Where(x => x.Name.StartsWith(userInput, StringComparison.InvariantCultureIgnoreCase));

            await ((SocketAutocompleteInteraction)Context.Interaction).RespondAsync(results.Take(25));
        }

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

        public static Embed buildAlreadyExistsError(string tagName)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Title = "Tag already exists",
                Description = $"The tag `{tagName}` already exists!"
            };
            embedBuilder.WithColor(new Color(255, 50, 50));

            return embedBuilder.Build();
        }

        public static Embed buildTagNotFoundError(string tagName)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder()
            {
                Title = "Tag not found",
                Description = $"The tag `{tagName}` could not be found in the database!"
            };
            embedBuilder.WithColor(new Color(255, 50, 50));

            return embedBuilder.Build();
        }

        public static String getTagInfoError()
        {
            return "An internal error occured while getting Tag Name information, please try again.";
        }
    }

    public class UserTagAssignable
    {
        // Apparently i have not found a more efficient way in asigning the tag name to an discord modal so i needed to temporarly save this assignment into a List

        public ulong userId { get; private set; }
        public ulong guildId { get; private set; }
        public string tagName { get; private set; }
        public UserTagAssignable(ulong guildID, ulong userID, string tagName)
        {
            this.userId = userID;
            this.guildId = guildID;
            this.tagName = tagName;
        }
    }
}
