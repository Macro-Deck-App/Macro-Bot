using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MacroBot.DataAccess.RepositoryInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
// ReSharper disable ClassNeverInstantiated.Global

namespace MacroBot.Commands.Tagging;

public class TaggingInteractions : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TaggingUtils _taggingUtils;
    private readonly ILogger _logger = Log.ForContext<TaggingInteractions>();

    public TaggingInteractions(IServiceScopeFactory serviceScopeFactory, TaggingUtils taggingUtils)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _taggingUtils = taggingUtils;
    }
    
    [ModalInteraction("tag_create_modal")]
    public async Task HandleCreateModal(TagCreateModal modal)
    {
        var assignable = TaggingCommands.createTagAssignments.Find(x => x.guildId == Context.Guild.Id && x.userId == Context.User.Id);
        if (assignable != null)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var tagRepository = scope.ServiceProvider.GetRequiredService<ITagRepository>();
            await tagRepository.CreateTag(assignable.tagName, modal.TagContent, assignable.userId, assignable.guildId);

            var embedBuilder = new EmbedBuilder()
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
            _logger.Error($"Could not find Tag name information for user {Context.User.Username} ({Context.User.Id}) in guild {Context.Guild.Name} ({Context.Guild.Id})!");
            await RespondAsync(_taggingUtils.getTagInfoError(), ephemeral: true);
        }
    }

    [ModalInteraction("tag_edit_modal")]
    public async Task HandleEditModal(TagEditModal modal)
    {
        var assignable = TaggingCommands.editTagAssignments.Find(x => x.guildId == Context.Guild.Id && x.userId == Context.User.Id);
        if (assignable != null)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var tagRepository = scope.ServiceProvider.GetRequiredService<ITagRepository>();
            await tagRepository.UpdateTag(assignable.tagName, modal.TagContent, assignable.guildId, assignable.userId);

            var embedBuilder = new EmbedBuilder()
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
            _logger.Error($"Could not find Tag name information for user {Context.User.Username} ({Context.User.Id}) in guild {Context.Guild.Name} ({Context.Guild.Id})!");
            await RespondAsync(_taggingUtils.getTagInfoError(), ephemeral: true);
        }
    }

    [ComponentInteraction("tag-delete-confirm")]
    public async Task TagDeleteConfirm()
    {
        var assignable = TaggingCommands.deleteTagAssignments.Find(x => x.guildId == Context.Guild.Id && x.userId == Context.User.Id);
        if (assignable != null)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var tagRepository = scope.ServiceProvider.GetRequiredService<ITagRepository>();
            await tagRepository.DeleteTag(assignable.tagName);

            await (Context.Interaction as SocketMessageComponent).Message.ModifyAsync(msg => msg.Components = new ComponentBuilder().Build());
            await RespondAsync($"Tag `{assignable.tagName}` successfully deleted");

            TaggingCommands.deleteTagAssignments.Remove(assignable);
        }
        else
        {
            _logger.Error($"Could not find Tag name information for user {Context.User.Username} ({Context.User.Id}) in guild {Context.Guild.Name} ({Context.Guild.Id})!");
            await RespondAsync(_taggingUtils.getTagInfoError(), ephemeral: true);
        }
    }
    [ComponentInteraction("tag-delete-cancel")]
    public async Task TagDeleteCancel()
    {
        var assignable = TaggingCommands.deleteTagAssignments.Find(x => x.guildId == Context.Guild.Id && x.userId == Context.User.Id);
        if (assignable != null)
        {
            TaggingCommands.deleteTagAssignments.Remove(assignable);
        }
        else
        {
            _logger.Error($"Could not find Tag name information for user {Context.User.Username} ({Context.User.Id}) in guild {Context.Guild.Name} ({Context.Guild.Id})!");
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