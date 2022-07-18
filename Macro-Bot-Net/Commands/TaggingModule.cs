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
        public static ConfigManager config = new ConfigManager("tagging");

        [SlashCommand("create","Create a new tag")]
        public async Task Create([Summary(description: "Name of the tag")] string name)
        {
            bool validPermission = false;

            var roles = ((SocketGuildUser)Context.User).Roles;
            JArray permissionRoles = config.getObject("PermissionManageTags").ToObject<JArray>();

            foreach (JValue roleID in permissionRoles)
            {
                if (roles.Contains()) ;
            }

            if (validPermission)
            {
                ModalBuilder modal = new ModalBuilder()
                .WithTitle("Create Tag")
                .WithCustomId("create_tag_modal")
                .AddTextInput("Tag Name", "tag_name", TextInputStyle.Short, "Enter tag name", required: true)
                .AddTextInput("Tag Content", "tag_content", TextInputStyle.Paragraph, "Enter tag content here", required: true);

                await RespondWithModalAsync(modal.Build());
            } else
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Title = "Cannot create tag",
                    Description = "You do not meet the permission requirements to run this command!"
                };
                string requiredFieldContent = "You need to have at least one of this roles:";
                foreach (string roleID in permissionRoles)
                {
                    requiredFieldContent += $"\n<@&{roleID}>";
                }

                embed.AddField("Required Roles", requiredFieldContent);

                await RespondAsync(embed: embed.Build());
            }
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
    }
}
