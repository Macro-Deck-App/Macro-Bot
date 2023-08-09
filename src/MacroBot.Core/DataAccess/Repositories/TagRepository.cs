using AutoMapper;
using MacroBot.Core.DataAccess.Entities;
using MacroBot.Core.DataAccess.RepositoryInterfaces;
using MacroBot.Core.Discord.Modules.Tagging;
using MacroBot.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MacroBot.Core.DataAccess.Repositories;

public class TagRepository : ITagRepository
{
    private readonly MacroBotContext _dbContext;
    private readonly IMapper _mapper;

    public TagRepository(MacroBotContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Tag?> GetTag(string name, ulong guild)
    {
        var existingTag = await _dbContext.GetNoTrackingSet<TagEntity>()
            .FirstOrDefaultAsync(x => x.Name.Equals(name) && x.Guild.Equals(guild));
        
        return existingTag is null ? null : _mapper.Map<TagEntity, Tag>(existingTag);
    }

    public async Task<bool> TagExists(string name, ulong guildId)
    {
        return await _dbContext.GetNoTrackingSet<TagEntity>()
            .AnyAsync(x => x.Name.Equals(name) && x.Guild.Equals(guildId));
    }

    public async Task CreateTag(string name, string content, ulong author, ulong guildId)
    {
        var tagExists = await TagExists(name, guildId);
        if (tagExists)
        {
            await UpdateTag(name, content, guildId, author);
            return;
        }

        var tagEntity = new TagEntity
        {
            Author = author,
            Guild = guildId,
            Name = name,
            Content = content
        };

        await _dbContext.CreateAsync(tagEntity);
    }

    public async Task UpdateTag(string name, string content, ulong guildId, ulong editor)
    {
        var existingTag = await _dbContext.GetNoTrackingSet<TagEntity>()
            .FirstOrDefaultAsync(x => x.Name.Equals(name) && x.Guild.Equals(guildId));
        
        if (existingTag is null)
        {
            await CreateTag(name, content, editor, guildId);
            return;
        }
        
        existingTag.Name = name;
        existingTag.Content = content;
        existingTag.Guild = guildId;
        existingTag.Author = editor;

        await _dbContext.UpdateAsync(existingTag);
    }

    public async Task DeleteTag(string name)
    {
        var existingTag = await _dbContext.GetNoTrackingSet<TagEntity>().FirstOrDefaultAsync(x => x.Name.Equals(name));
        if (existingTag is null)
        {
            return;
        }

        await _dbContext.DeleteAsync(existingTag);
    }

    public async Task<IEnumerable<Tag>> GetTagsForGuild(ulong guildId)
    {
        var guildTags = await _dbContext.GetNoTrackingSet<TagEntity>()
            .Where(x => x.Guild == guildId)
            .ToArrayAsync();
        return _mapper.Map<IEnumerable<TagEntity>, IEnumerable<Tag>>(guildTags);
    }

    public async Task<IEnumerable<Tag>> GetTagsFromUser(ulong guildId, ulong userId)
    {
        var userTags = await _dbContext.GetNoTrackingSet<TagEntity>()
            .Where(x => x.Guild.Equals(guildId) && x.Author.Equals(userId))
            .ToArrayAsync();
        return _mapper.Map<IEnumerable<TagEntity>, IEnumerable<Tag>>(userTags);
    }
}