using System.Timers;
using MacroBot.ServiceInterfaces;
using Serilog;
using ILogger = Serilog.ILogger;
using Timer = System.Timers.Timer;

namespace MacroBot.Services;

public class TimerService : ITimerService, IHostedService
{
    private readonly ILogger _logger = Log.ForContext<TimerService>();
    
    public event EventHandler? DailyTimerElapsed;
    public event EventHandler? HourTimerElapsed;
    public event EventHandler? MinuteTimerElapsed;
    public event EventHandler? FiveMinuteTimerElapsed;


    private DateTime _hourTimerLastElapsed = default;
    private DateTime _dailyTimerLastElapsed = default;

    private readonly Timer _timer;
    
    public TimerService()
    {
        _timer = new Timer
        {
            Interval = 1000 * 60
        };
        _timer.Elapsed += TimerOnElapsed;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Stop();
        return Task.CompletedTask;
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        _logger.Verbose("Minute timer elapsed");
        MinuteTimerElapsed?.Invoke(this, EventArgs.Empty);
        if (DateTime.Now - _hourTimerLastElapsed >= TimeSpan.FromMinutes(5))
        {
            _logger.Verbose("5 Minute timer elapsed");
            HourTimerElapsed?.Invoke(this, EventArgs.Empty);
            _hourTimerLastElapsed = DateTime.Now;
        }
        if (DateTime.Now - _hourTimerLastElapsed >= TimeSpan.FromHours(1))
        {
            _logger.Verbose("Hour timer elapsed");
            HourTimerElapsed?.Invoke(this, EventArgs.Empty);
            _hourTimerLastElapsed = DateTime.Now;
        }

        if (DateTime.Now.Hour == 5 && DateTime.Now - _dailyTimerLastElapsed >= TimeSpan.FromHours(23))
        {
            _logger.Verbose("Daily timer elapsed");
            DailyTimerElapsed?.Invoke(this, EventArgs.Empty);
            _dailyTimerLastElapsed = DateTime.Now;
        }
    }

}