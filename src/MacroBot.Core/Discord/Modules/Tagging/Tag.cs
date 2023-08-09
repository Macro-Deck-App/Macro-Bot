namespace MacroBot.Core.Discord.Modules.Tagging;

public class Tag
{
	public ulong Author { get; set; }
	public ulong Guild { get; set; }
	public string Name { get; set; }
	public string Content { get; set; }
	public DateTime Created { get; set; }
	public DateTime? LastEdited { get; set; }
}