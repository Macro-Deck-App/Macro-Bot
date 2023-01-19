namespace MacroBot.Models.Status;

public class StatusCheckFinishedEventArgs
{
    public IEnumerable<StatusCheckResult> Results { get; set; }
}