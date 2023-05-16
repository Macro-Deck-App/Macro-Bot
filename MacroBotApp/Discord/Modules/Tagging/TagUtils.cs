using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MacroBot.DataAccess.RepositoryInterfaces;

namespace MacroBot.Discord.Modules.Tagging;

public class TaggingUtils
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TaggingUtils(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task AutoCompleteTag(SocketInteractionContext Context)
    {
        var userInput = (Context.Interaction as SocketAutocompleteInteraction).Data.Current.Value.ToString();

        List<AutocompleteResult> resultList = new();
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var tagRepository = scope.ServiceProvider.GetRequiredService<ITagRepository>();
        foreach (var tag in await tagRepository.GetTagsForGuild(Context.Guild.Id))
            resultList.Add(new AutocompleteResult(tag.Name, tag.Name));
        var results = resultList.AsEnumerable()
            .Where(x => x.Name.StartsWith(userInput, StringComparison.InvariantCultureIgnoreCase));

        await ((SocketAutocompleteInteraction)Context.Interaction).RespondAsync(results.Take(25));
    }

    public bool CheckPermissions(ulong[] permissions, SocketGuildUser user)
    {
        var userRoles = user.Roles.ToList<SocketRole>();

        foreach (var permission in permissions)
            if (userRoles.Find(x => x.Id.ToString() == permission.ToString()) != null)
                return true;
        return false;
    }

    public Embed buildPermissionError(ulong[] permissions)
    {
        var embedBuilder = new EmbedBuilder
        {
            Title = "Invalid Permissions",
            Description = "You do not meet the permission requirements to run this command!"
        };
        var permissionList = string.Empty;
        foreach (var permissionID in permissions) permissionList += $"\n<@&{permissionID.ToString()}>";
        embedBuilder.AddField("Required Roles", $"At least one of those roles is required:{permissionList}");
        embedBuilder.WithColor(new Color(255, 50, 50));

        return embedBuilder.Build();
    }

    public Embed buildAlreadyExistsError(string tagName)
    {
        var embedBuilder = new EmbedBuilder
        {
            Title = "Tag already exists",
            Description = $"The tag `{tagName}` already exists!"
        };
        embedBuilder.WithColor(new Color(255, 50, 50));

        return embedBuilder.Build();
    }

    public Embed buildTagNotFoundError(string tagName)
    {
        var embedBuilder = new EmbedBuilder
        {
            Title = "Tag not found",
            Description = $"The tag `{tagName}` could not be found in the database!"
        };
        embedBuilder.WithColor(new Color(255, 50, 50));

        return embedBuilder.Build();
    }

    public string getTagInfoError()
    {
        return "An internal error occured while getting Tag Name information, please try again.";
    }
}

public class UserTagAssignable
{
    public UserTagAssignable(ulong guildID, ulong userID, string tagName)
    {
        userId = userID;
        guildId = guildID;
        this.tagName = tagName;
    }
    // Apparently i have not found a more efficient way in asigning the tag name to an discord modal so i needed to temporarly save this assignment into a List

    public ulong userId { get; private set; }
    public ulong guildId { get; private set; }
    public string tagName { get; private set; }
}