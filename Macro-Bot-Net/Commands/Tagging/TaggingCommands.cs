using Develeon64.MacroBot.Models;
using Develeon64.MacroBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Develeon64.MacroBot.Commands.Tagging
{
    [Group("tag", "Tag system")]
    public class TaggingCommands : InteractionModuleBase<SocketInteractionContext>
    {
        // Assignable Lists
        public static List<UserTagAssignable> createTagAssignments = new();
        public static List<UserTagAssignable> deleteTagAssignments = new();
        public static List<UserTagAssignable> editTagAssignments = new();

        // Autocomplete Arguments
        [AutocompleteCommand("tag", "delete")]
        public async Task AutoCompleteDelete()
        {
            await TaggingUtils.AutoCompleteTag(Context);
        }

        [AutocompleteCommand("tag", "edit")]
        public async Task AutoCompleteEdit()
        {
            await TaggingUtils.AutoCompleteTag(Context);
        }

        [AutocompleteCommand("tag", "view")]
        public async Task AutoCompleteView()
        {
            await TaggingUtils.AutoCompleteTag(Context);
        }

        [AutocompleteCommand("tag", "raw")]
        public async Task AutoCompleteRaw()
        {
            await TaggingUtils.AutoCompleteTag(Context);
        }

        // Commands
        [SlashCommand("create", "Create a new tag")]
        public async Task Create([Summary(description: "Name of the tag")] string name)
        {
            // Handle Permissions
            SocketGuildUser guildUser = Context.Guild.GetUser(Context.User.Id);
            ulong[] requiredPermissions = ConfigManager.CommandsConfig.Tagging.PermissionManageTags;
            if (!TaggingUtils.checkPermissions(requiredPermissions, guildUser))
            {

                await RespondAsync(embed: TaggingUtils.buildPermissionError(requiredPermissions), ephemeral: true);
                return;
            }

            // Handle Already Exists
            if (await DatabaseManager.TagExists(name, Context.Guild.Id))
            {
                await RespondAsync(embed: TaggingUtils.buildAlreadyExistsError(name), ephemeral: true);
                return;
            }

            // Handle Assignables
            UserTagAssignable? assignable = createTagAssignments.Find(x => x.guildId == Context.Guild.Id && x.userId == Context.User.Id);
            if (assignable != null)
            {
                createTagAssignments.Remove(assignable);
            }
            createTagAssignments.Add(new UserTagAssignable(Context.Guild.Id, Context.User.Id, name));

            // Send Modal Prompt
            await RespondWithModalAsync<TagCreateModal>("tag_create_modal");
        }


        [SlashCommand("delete", "Delete an existing tag")]
        public async Task Delete([Summary("tag"), Autocomplete] string tagName)
        {
            // Handle Permissions
            SocketGuildUser guildUser = Context.Guild.GetUser(Context.User.Id);
            ulong[] requiredPermissions = ConfigManager.CommandsConfig.Tagging.PermissionManageTags;
            if (!TaggingUtils.checkPermissions(requiredPermissions, guildUser))
            {

                await RespondAsync(embed: TaggingUtils.buildPermissionError(requiredPermissions), ephemeral: true);
                return;
            }

            // Get Tag from Database
            Tag tag = await DatabaseManager.GetTag(tagName, Context.Guild.Id);

            // Handle Tag Not Found
            if (tag == null)
            {
                await RespondAsync(embed: TaggingUtils.buildTagNotFoundError(tagName), ephemeral: true);
                return;
            }

            // Handle Assignables
            UserTagAssignable? assignable = deleteTagAssignments.Find(x => x.guildId == Context.Guild.Id && x.userId == Context.User.Id);
            if (assignable != null)
            {
                deleteTagAssignments.Remove(assignable);
            }
            deleteTagAssignments.Add(new UserTagAssignable(Context.Guild.Id, Context.User.Id, tagName));

            // Send Confirm Prompt
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = "Confirm Tag Deletion",
                Description = $"Do you really want to delete following tag?\n\n**{tag.Name}**\n{tag.Content}",
            };
            embed.WithColor(new Color(255, 50, 50));

            ComponentBuilder componentBuilder = new ComponentBuilder()
                .WithButton("Delete Tag", "tag-delete-confirm", ButtonStyle.Success)
                .WithButton("Cancel Deletion", "tag-delete-cancel", ButtonStyle.Danger);

            await RespondAsync(embed: embed.Build(), components: componentBuilder.Build());
        }


        [SlashCommand("edit", "Delete an existing tag")]
        public async Task Edit([Summary("tag"), Autocomplete] string tagName)
        {
            // Handle Permissions
            SocketGuildUser guildUser = Context.Guild.GetUser(Context.User.Id);
            ulong[] requiredPermissions = ConfigManager.CommandsConfig.Tagging.PermissionManageTags;
            if (!TaggingUtils.checkPermissions(requiredPermissions, guildUser))
            {

                await RespondAsync(embed: TaggingUtils.buildPermissionError(requiredPermissions), ephemeral: true);
                return;
            }

            // Get Tag from Database
            Tag tag = await DatabaseManager.GetTag(tagName, Context.Guild.Id);

            // Handle Tag Not Found
            if (tag == null)
            {
                await RespondAsync(embed: TaggingUtils.buildTagNotFoundError(tagName), ephemeral: true);
                return;
            }

            // Handle Assignables
            UserTagAssignable? assignable = editTagAssignments.Find(x => x.guildId == Context.Guild.Id && x.userId == Context.User.Id);
            if (assignable != null)
            {
                editTagAssignments.Remove(assignable);
            }
            editTagAssignments.Add(new UserTagAssignable(Context.Guild.Id, Context.User.Id, tagName));


            // Send Modal Prompt
            await RespondWithModalAsync<TagEditModal>("tag_edit_modal");
        }


        [SlashCommand("view", "View a tag")]
        public async Task View([Summary("tag"), Autocomplete] string tagName)
        {
            // Get Tag from Database
            Tag tag = await DatabaseManager.GetTag(tagName, Context.Guild.Id);

            // Handle Tag Not Found
            if (tag == null)
            {
                await RespondAsync(embed: TaggingUtils.buildTagNotFoundError(tagName), ephemeral: true);
                return;
            }

            // Build Viewer embed
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = tagName,
                Description = tag.Content,
            };

            String footerText = "";
            String footerAvatarUrl = "";

            SocketGuildUser author = Context.Guild.GetUser(tag.Author);
            if (author != null)
            {
                footerText += $"by {author.DisplayName}";
                footerAvatarUrl = author.GetAvatarUrl();
            }
            else
            {
                footerText += $"by ${tag.Author}";
            }

            footerText += " | Last Edited";
            embed.WithTimestamp((DateTimeOffset)tag.LastEdited);

            embed.WithFooter(footerText, footerAvatarUrl);

            // Reply with Viewer embed
            await RespondAsync(embed: embed.Build());
        }


        [SlashCommand("list", "List all tags")]
        public async Task List([Summary("user", "The user who created the tags")] IUser? user = null)
        {
            // Get tags from database
            List<Tag> tagList = new();
            String desc = "";
            String title = "";

            // Check if user argument is given
            if (user != null)
            {
                tagList = await DatabaseManager.GetTagsFromUser(Context.Guild.Id, user.Id);
                title = $"Tag list";
                desc += $"Show tags by <@{user.Id}>:\n\n";
            }
            else
            {
                tagList = await DatabaseManager.GetTagsForGuild(Context.Guild.Id);
                title = "Tags for " + Context.Guild.Name;
            }

            // Check if any tags were found
            if (tagList.Count == 0)
            {
                desc += "**No tags found!**";
            }

            // Build tag list
            foreach (Tag tag in tagList)
            {
                desc += $"`{tag.Name}`";
                if (user == null) desc += $" by <@{tag.Author}>";
                desc += "\n";
            }

            // Build viwer embed
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = title,
                Description = desc,
            };

            // Reply with viewer embed
            await RespondAsync(embed: embed.Build());
        }


        [SlashCommand("raw", "View a tags raw content")]
        public async Task ViewRaw([Summary("tag"), Autocomplete] string tagName)
        {
            // Get Tag from Database
            Tag tag = await DatabaseManager.GetTag(tagName, Context.Guild.Id);

            // Handle Tag Not Found
            if (tag == null)
            {
                await RespondAsync(embed: TaggingUtils.buildTagNotFoundError(tagName), ephemeral: true);
                return;
            }

            // Build viewer embed
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = tagName + " | Raw Content",
                Description = tag.Content.Replace("<", "\\<").Replace("*", "\\*").Replace("_", "\\_").Replace("`", "\\`").Replace(":", "\\:"),
            };
            String footerText = "";
            String footerAvatarUrl = "";

            SocketGuildUser author = Context.Guild.GetUser(tag.Author);
            if (author != null)
            {
                footerText += $"by {author.DisplayName}";
                footerAvatarUrl = author.GetAvatarUrl();
            }
            else
            {
                footerText += $"by ${tag.Author}";
            }

            footerText += " | Last Edited";
            embed.WithTimestamp((DateTimeOffset)tag.LastEdited);

            embed.WithFooter(footerText, footerAvatarUrl);

            // Reply with viewer embed
            await RespondAsync(embed: embed.Build());
        }
    }
}
