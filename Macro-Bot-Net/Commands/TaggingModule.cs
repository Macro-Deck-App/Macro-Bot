using Develeon64.MacroBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Develeon64.MacroBot.Commands
{
    [Group("tag", "Tag system")]
    public class TaggingModule : InteractionModuleBase<SocketInteractionContext>
    {

        [SlashCommand("create","Create a new tag")]
        public async Task Create([Summary(description: "Name of the tag")] string name)
        {
            /*
            SocketGuildUser guildUser = Context.Guild.GetUser(Context.User.Id);
            String permisisonGroup = "PermissionManageTags";
            if (!checkPermissions(permisisonGroup, guildUser)) {
                EmbedBuilder errorEmbed = new()
                {
                    Title = "Invalid Permissions",
                    Description = "You do not have the needed permissions to run this command!"
                };

                String permissionList = "";
                foreach (JValue permissionID in config.getObject(permisisonGroup).ToObject<JArray>()) permissionList += $"\n<@&{permissionID.ToString()}>";
                errorEmbed.AddField("Required Roles", $"At least one of those roles is required:{permissionList}");

                await RespondAsync(embed: errorEmbed.Build(),ephemeral: true);
                return;
            }
            */

            ModalBuilder modal = new ModalBuilder()
                .WithTitle("Create Tag")
                .WithCustomId("create_tag_modal")
                .AddTextInput("Tag Name", "tag_name", TextInputStyle.Short, "Enter tag name", required: true)
                .AddTextInput("Tag Content", "tag_content", TextInputStyle.Paragraph, "Enter tag content here", required: true);

            await RespondWithModalAsync(modal.Build());
        }

        [SlashCommand("delete", "Delete an existing tag")]
        public async Task Delete()
        {
            await RespondAsync("WIP");
        }

        [SlashCommand("edit", "Delete an existing tag")]
        public async Task Edit()
        {
            await RespondAsync("WIP");
        }

        [SlashCommand("view", "View a tag")]
        public async Task View()
        {
            await RespondAsync("WIP");
        }

        private static Boolean checkPermissions(string permissionGroup,SocketGuildUser user)
        {
            /*
            JArray manageTagsPermissions = config.getObject(permissionGroup).ToObject<JArray>();
            List<SocketRole> userRoles = user.Roles.ToList<SocketRole>();
            
            foreach (JValue permission in manageTagsPermissions)
            {
                if (userRoles.Find(x => x.Id.ToString() == permission.ToString()) != null)
                {
                    return true;
                }
            }
            return false;
            */
            return true;
        }

        [ModalInteraction("create_tag_modal")]
        public async Task HandleCreateModal(Modal modal)
        {
            Console.WriteLine(modal);
            
            await RespondAsync("test");
        }
    }
}
