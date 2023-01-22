using MacroBot.Models.Status;

namespace MacroBot.ServiceInterfaces;

public interface IStatusCheckService
{
    public event EventHandler<StatusCheckFinishedEventArgs> StatusCheckFinished;
    public event EventHandler StatusOfItemChanged;
    public event EventHandler ItemStatusInCollectionChanged;
    public IEnumerable<StatusCheckResult>? LastStatusCheckResults { get; }
    public DateTime LastStatusCheck { get; }
}