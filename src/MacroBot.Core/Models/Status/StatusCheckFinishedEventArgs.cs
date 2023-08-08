namespace MacroBot.Core.Models.Status;

public class StatusCheckFinishedEventArgs
{
    public IEnumerable<StatusCheckResult> Results { get; set; }
}