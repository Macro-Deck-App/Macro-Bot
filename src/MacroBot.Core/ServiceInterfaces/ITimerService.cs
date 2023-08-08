namespace MacroBot.Core.ServiceInterfaces;

public interface ITimerService
{
    public event EventHandler DailyTimerElapsed;
    public event EventHandler HourTimerElapsed;
    public event EventHandler MinuteTimerElapsed;
    public event EventHandler FiveMinuteTimerElapsed;
}