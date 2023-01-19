using MacroBot.Discord.Modules.Tagging;

namespace MacroBot.DataAccess.RepositoryInterfaces;

public interface ITagRepository
{
    public Task<Tag?> GetTag(string name, ulong guild);
    public Task<bool> TagExists(string name, ulong guildId);
    public Task CreateTag(string name, string content, ulong author, ulong guildId);
    public Task UpdateTag(string name, string content, ulong guildId, ulong editor);
    public Task DeleteTag(string name);
    public Task<IEnumerable<Tag>> GetTagsForGuild(ulong guildId);
    public Task<IEnumerable<Tag>> GetTagsFromUser(ulong guildId, ulong userId);
}