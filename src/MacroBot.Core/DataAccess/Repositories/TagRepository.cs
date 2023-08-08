using AutoMapper;
using MacroBot.Core.DataAccess.Entities;
using MacroBot.Core.DataAccess.RepositoryInterfaces;
using MacroBot.Core.Discord.Modules.Tagging;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroBot.Core.DataAccess.Repositories;

public class TagRepository : ITagRepository
{
    private readonly ILogger _logger = Log.ForContext<TagRepository>();
    
    private readonly MacroBotContext _dbContext;
    private readonly IMapper _mapper;

    public TagRepository(MacroBotContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Tag?> GetTag(string name, ulong guild)
    {
        var existingTag =
            await _dbContext.TagEntities.FirstOrDefaultAsync(x => x.Name.Equals(name) && x.Guild.Equals(guild));
        if (existingTag is null)
        {
            _logger.Warning("Tag {TagName} not found");
            return null;
        }

        var mappedTag = _mapper.Map<TagEntity, Tag>(existingTag);
        return mappedTag;
    }

    public async Task<bool> TagExists(string name, ulong guildId)
    {
        var tagExist = await _dbContext.TagEntities.AnyAsync(x => x.Name.Equals(name) && x.Guild.Equals(guildId));
        return tagExist;
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
            Content = content,
            LastEdited = default
        };
        await _dbContext.TagEntities.AddAsync(tagEntity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateTag(string name, string content, ulong guildId, ulong editor)
    {
        var existingTag =
            await _dbContext.TagEntities.FirstOrDefaultAsync(x => x.Name.Equals(name) && x.Guild.Equals(guildId));
        if (existingTag is null)
        {
            await CreateTag(name, content, editor, guildId);
            return;
        }
        existingTag.Name = name;
        existingTag.Content = content;
        existingTag.Guild = guildId;
        existingTag.Author = editor;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteTag(string name)
    {
        var existingTag = await _dbContext.TagEntities.FirstOrDefaultAsync(x => x.Name.Equals(name));
        if (existingTag is null)
        {
            _logger.Warning("Cannot delete tag {TagName} - Tag not found", name);
            return;
        }
        _dbContext.TagEntities.Remove(existingTag);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Tag>> GetTagsForGuild(ulong guildId)
    {
        var guildTags = await Queryable.Where<TagEntity>(_dbContext.TagEntities, x => x.Guild == guildId).ToArrayAsync();
        if (guildTags.Length == 0)
        {
            return Enumerable.Empty<Tag>();
        }
        var guildTagsMapped = _mapper.Map<IEnumerable<TagEntity>, IEnumerable<Tag>>(guildTags);
        return guildTagsMapped;
    }

    public async Task<IEnumerable<Tag>> GetTagsFromUser(ulong guildId, ulong userId)
    {
        var userTags = await Queryable.Where<TagEntity>(_dbContext.TagEntities, x => x.Guild.Equals(guildId) && x.Author.Equals(userId))
            .ToArrayAsync();
        if (userTags.Length == 0)
        {
            return Enumerable.Empty<Tag>();
        }
        var userTagsMapped = _mapper.Map<IEnumerable<TagEntity>, IEnumerable<Tag>>(userTags);
        return userTagsMapped;
    }
}