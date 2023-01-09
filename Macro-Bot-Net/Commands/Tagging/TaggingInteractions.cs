using Develeon64.MacroBot.Logging;
using Develeon64.MacroBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Develeon64.MacroBot.Commands.Tagging
{
    public class TaggingInteractions : InteractionModuleBase<SocketInteractionContext>
    {
        [ModalInteraction("tag_create_modal")]
        public async Task HandleCreateModal(TagCreateModal modal)
        {
            UserTagAssignable? assignable = TaggingCommands.createTagAssignments.Find(x => x.guildId == Context.Guild.Id && x.userId == Context.User.Id);
            if (assignable != null)
            {
                await DatabaseManager.CreateTag(assignable.tagName, modal.TagContent, assignable.userId, assignable.guildId);

                EmbedBuilder embedBuilder = new EmbedBuilder()
                {
                    Title = "Tag Created",
                    Description = $"Successfully created tag `{assignable.tagName}`\n\n**Tag Content**\n{modal.TagContent}"
                };
                embedBuilder.WithColor(new Color(50, 255, 50));

                TaggingCommands.createTagAssignments.Remove(assignable);

                await RespondAsync(embed: embedBuilder.Build());
            }
            else
            {
                Logger.Warning(Modules.Tags, $"Could not find Tag name information for user {Context.User.Username} ({Context.User.Id}) in guild {Context.Guild.Name} ({Context.Guild.Id})!");
                await RespondAsync(TaggingUtils.getTagInfoError(), ephemeral: true);
            }
        }

        [ModalInteraction("tag_edit_modal")]
        public async Task HandleEditModal(TagEditModal modal)
        {
            UserTagAssignable? assignable = TaggingCommands.editTagAssignments.Find(x => x.guildId == Context.Guild.Id && x.userId == Context.User.Id);
            if (assignable != null)
            {
                await DatabaseManager.UpdateTag(assignable.tagName, modal.TagContent, assignable.guildId, assignable.userId);

                EmbedBuilder embedBuilder = new EmbedBuilder()
                {
                    Title = "Tag Edited",
                    Description = $"Edited tag `{assignable.tagName}`\n\n**Tag Content**\n{modal.TagContent}"
                };
                embedBuilder.WithColor(new Color(50, 255, 50));

                TaggingCommands.editTagAssignments.Remove(assignable);

                await RespondAsync(embed: embedBuilder.Build());
            }
            else
            {
                Logger.Warning(Modules.Tags, $"Could not find Tag name information for user {Context.User.Username} ({Context.User.Id}) in guild {Context.Guild.Name} ({Context.Guild.Id})!");
                await RespondAsync(TaggingUtils.getTagInfoError(), ephemeral: true);
            }
        }

        [ComponentInteraction("tag-delete-confirm")]
        public async Task TagDeleteConfirm()
        {
            UserTagAssignable? assignable = TaggingCommands.deleteTagAssignments.Find(x => x.guildId == Context.Guild.Id && x.userId == Context.User.Id);
            if (assignable != null)
            {
                await DatabaseManager.DeleteTag(assignable.tagName);

                await (Context.Interaction as SocketMessageComponent).Message.ModifyAsync(msg => msg.Components = new ComponentBuilder().Build());
                await RespondAsync($"Tag `{assignable.tagName}` successfully deleted");

                TaggingCommands.deleteTagAssignments.Remove(assignable);
            }
            else
            {
                Logger.Warning(Modules.Tags, $"Could not find Tag name information for user {Context.User.Username} ({Context.User.Id}) in guild {Context.Guild.Name} ({Context.Guild.Id})!");
                await RespondAsync(TaggingUtils.getTagInfoError(), ephemeral: true);
            }
        }
        [ComponentInteraction("tag-delete-cancel")]
        public async Task TagDeleteCancel()
        {
            UserTagAssignable? assignable = TaggingCommands.deleteTagAssignments.Find(x => x.guildId == Context.Guild.Id && x.userId == Context.User.Id);
            if (assignable != null)
            {
                TaggingCommands.deleteTagAssignments.Remove(assignable);
            }
            else
            {
                Logger.Warning(Modules.Tags, $"Could not find Tag name information for user {Context.User.Username} ({Context.User.Id}) in guild {Context.Guild.Name} ({Context.Guild.Id})!");
            }

            await (Context.Interaction as SocketMessageComponent).Message.ModifyAsync(msg => msg.Components = new ComponentBuilder().Build());
            await RespondAsync($"Tag deletion cancelled!");
        }
    }

    public class TagCreateModal : IModal
    {
        public string Title => "Create Tag";
        [InputLabel("Tag Content")]
        [ModalTextInput("tag_create_content", TextInputStyle.Paragraph, minLength: 1, maxLength: 4000)]
        public string TagContent { get; set; }
    }

    public class TagEditModal : IModal
    {
        public string Title => "Edit Tag";
        [InputLabel("Tag Content")]
        [ModalTextInput("tag_create_content", TextInputStyle.Paragraph, minLength: 1, maxLength: 4000, placeholder: "Sadly we cannot provide the existing tag content here automatically due to Discord limitations")]
        public string TagContent { get; set; }
    }
}
