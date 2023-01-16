namespace MacroBot.Models;

public class StatusCheckFinishedEventArgs
{
    public IEnumerable<StatusCheckResult> Results { get; set; }
}