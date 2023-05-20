namespace MacroBot.DataAccess.Entities;

public class TagEntity
{
	public int Id { get; set; }
	public ulong Author { get; set; }
	public ulong Guild { get; set; }
	public string Name { get; set; }
	public string Content { get; set; }
	public DateTime LastEdited { get; set; }
}