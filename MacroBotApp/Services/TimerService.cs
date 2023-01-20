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

    private DateTime hourTimerLastElapsed = default;
    private DateTime dailyTimerLastElapsed = default;

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
        if (DateTime.Now - hourTimerLastElapsed >= TimeSpan.FromMinutes(1))
        {
            _logger.Verbose("Hour timer elapsed");
            HourTimerElapsed?.Invoke(this, EventArgs.Empty);
            hourTimerLastElapsed = DateTime.Now;
        }

        if (DateTime.Now.Hour == 5 && DateTime.Now - dailyTimerLastElapsed >= TimeSpan.FromHours(23))
        {
            _logger.Verbose("Daily timer elapsed");
            DailyTimerElapsed?.Invoke(this, EventArgs.Empty);
            dailyTimerLastElapsed = DateTime.Now;
        }
    }

}