using Discord;
using Discord.Interactions;
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
            ModalBuilder modal = new ModalBuilder()
                .WithTitle("Create Tag")
                .WithCustomId("create_tag_modal")
                .AddTextInput("Tag Content", "tag_content", TextInputStyle.Paragraph, "Enter tag content here", required: true);

            await RespondWithModalAsync(modal.Build());
        }

        [SlashCommand("delete", "Delete an existing tag")]
        public async Task Delete()
        {

        }

        [SlashCommand("edit", "Delete an existing tag")]
        public async Task Edit()
        {

        }

        [SlashCommand("view", "View a tag")]
        public async Task View()
        {

        }
    }
}
