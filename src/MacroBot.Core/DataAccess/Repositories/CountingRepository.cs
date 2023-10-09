using AutoMapper;
using MacroBot.Core.DataAccess.Entities;
using MacroBot.Core.DataAccess.RepositoryInterfaces;
using MacroBot.Core.Discord.Modules.Tagging;
using MacroBot.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MacroBot.Core.DataAccess.Repositories;

public class CountingRepository : ICountingRepository {
    private readonly MacroBotContext _dbContext;
    private readonly IMapper _mapper;

    public CountingRepository(MacroBotContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<CountingEntity?> GetCurrentCount() {
        var existingCount = await _dbContext.GetNoTrackingSet<CountingEntity>()
            .FirstOrDefaultAsync();

        if (existingCount is null)
        {
            existingCount = new CountingEntity {
                CurrentAuthor = 0,
                CurrentCount = 0,
                HighScore = 0
            };
        }

        return existingCount;
    }

    public async Task SetCount(long count, ulong user) {
        var existingCount = await _dbContext.GetNoTrackingSet<CountingEntity>()
            .FirstOrDefaultAsync();

        if (existingCount is null)
        {
            await CreateCount(count, user);
            return;
        }

        existingCount.CurrentCount = count;
        existingCount.CurrentAuthor = user;

        await _dbContext.UpdateAsync(existingCount);
    }   

    public async Task CreateCount(long count, ulong user) {
        var countingEntity = new CountingEntity
        {
            CurrentAuthor = user,
            CurrentCount = count,
            HighScore = 0
        };

        await _dbContext.CreateAsync(countingEntity);
    }

    public async Task SetCountHighScore(long highScore) {
        var existingCount = await _dbContext.GetNoTrackingSet<CountingEntity>()
            .FirstOrDefaultAsync();

        if (existingCount is null)
        {
            return;
        }

        existingCount.HighScore = highScore;

        await _dbContext.UpdateAsync(existingCount);
    } 
}