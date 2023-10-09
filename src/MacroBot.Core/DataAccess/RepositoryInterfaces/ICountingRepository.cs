using MacroBot.Core.DataAccess.Entities;
using MacroBot.Core.Discord.Modules.Tagging;

namespace MacroBot.Core.DataAccess.RepositoryInterfaces;

public interface ICountingRepository
{
    public Task<CountingEntity?> GetCurrentCount();
    public Task SetCount(long c, ulong u);
    public Task CreateCount(long c, ulong u);
    public Task SetCountHighScore(long h);
}