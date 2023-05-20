using MacroBot.Models.Status;

namespace MacroBot.ServiceInterfaces;

public interface IStatusCheckService
{
	public IEnumerable<StatusCheckResult>? LastStatusCheckResults { get; }
	public DateTime LastStatusCheck { get; }
	public event EventHandler<StatusCheckFinishedEventArgs> StatusCheckFinished;
	public event EventHandler StatusOfItemChanged;
	public event EventHandler ItemStatusInCollectionChanged;
}