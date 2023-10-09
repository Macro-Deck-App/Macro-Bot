namespace MacroBot.Core.DataAccess.Entities;

public class CountingEntity : BaseCreatedUpdatedEntity
{
    public ulong CurrentAuthor { get; set; } = 0;
    public long CurrentCount { get; set; } = 0;
    public long HighScore { get; set; } = 0;
}