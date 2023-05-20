namespace MacroBot.DataAccess.Entities;

public class ReportEntity
{
	public string Id { get; set; }
	public ulong Reporter { get; set; }
	public ulong Guild { get; set; }
	public ulong User { get; set; }
	public ulong? Channel { get; set; }
	public ulong? Message { get; set; }
	public string Content { get; set; }
	public DateTime Reported { get; set; }
}