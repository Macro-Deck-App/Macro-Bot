namespace MacroBot.Core.DataAccess.Entities;

public class TagEntity : BaseCreatedUpdatedEntity
{
    public ulong Author { get; set; }
    public ulong Guild { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}